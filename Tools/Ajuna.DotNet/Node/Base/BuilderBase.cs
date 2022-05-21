using Ajuna.NetApi.Model.Meta;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ajuna.DotNet.Node.Base
{

   public abstract class BuilderBase
   {
      public static List<string> Files = new();

      public uint Id { get; }

      public Dictionary<uint, (string, List<string>)> TypeDict { get; }

      public bool Success { get; set; }

      public string NamespaceName { get; protected set; }

      internal string FileName { get; set; }

      public string ClassName { get; set; }

      public string ReferenzName { get; set; }

      public string ProjectName { get; private set; }

      public CodeNamespace ImportsNamespace { get; set; }

      public CodeCompileUnit TargetUnit { get; set; }

      public abstract BuilderBase Create();

      public BuilderBase(string projectName, uint id, Dictionary<uint, (string, List<string>)> typeDict)
      {
         ProjectName = projectName;
         Id = id;
         TypeDict = typeDict;
         ImportsNamespace = new()
         {
            Imports = {
               new CodeNamespaceImport("Ajuna.NetApi.Model.Types.Base"),
               new CodeNamespaceImport("System.Collections.Generic")
            }
         };

         TargetUnit = new CodeCompileUnit();
         TargetUnit.Namespaces.Add(ImportsNamespace);

         Success = true;
      }

      public (string, List<string>) GetFullItemPath(uint typeId)
      {
         if (!TypeDict.TryGetValue(typeId, out (string, List<string>) fullItem))
         {
            Success = false;
            fullItem = ("Unknown", new List<string>() { "Unknown" });
         }
         else
         {
            fullItem.Item2.ForEach(p => ImportsNamespace.Imports.Add(new CodeNamespaceImport(p)));
         }

         return fullItem;
      }

      public static CodeCommentStatementCollection GetComments(string[] docs, NodeType typeDef = null,
          string typeName = null)
      {
         CodeCommentStatementCollection comments = new();
         comments.Add(new CodeCommentStatement("<summary>", true));

         if (typeDef != null)
         {
            var path = typeDef.Path != null ? "[" + string.Join('.', typeDef.Path) + "]" : "";
            comments.Add(new CodeCommentStatement($">> {typeDef.Id} - {typeDef.TypeDef}{path}", true));
         }

         if (typeName != null)
         {
            comments.Add(new CodeCommentStatement($">> {typeName}", true));
         }

         if (docs != null)
         {
            foreach (var doc in docs)
            {
               comments.Add(new CodeCommentStatement(doc, true));
            }
         }

         comments.Add(new CodeCommentStatement("</summary>", true));
         return comments;
      }

      public CodeMemberMethod SimpleMethod(string name, string returnType = null, object returnExpression = null)
      {
         CodeMemberMethod nameMethod = new()
         {
            Attributes = MemberAttributes.Public | MemberAttributes.Override,
            Name = name
         };

         if (returnType != null)
         {
            nameMethod.ReturnType = new CodeTypeReference(returnType);
            CodeMethodReturnStatement nameReturnStatement = new()
            {
               Expression = new CodePrimitiveExpression(returnExpression)
            };
            nameMethod.Statements.Add(nameReturnStatement);
         }

         return nameMethod;
      }

      public virtual (string, List<string>) Build(bool write, out bool success, string basePath = null)
      {
         success = Success;
         if (write && Success)
         {
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CodeGeneratorOptions options = new()
            {
               BracingStyle = "C"
            };

            string path = GetPath(basePath);

            Directory.CreateDirectory(Path.GetDirectoryName(path));

            if (Files.Contains(path))
            {
               Console.WriteLine($"Overwriting[BUG]: {path}");
               //path += _index++;
            }
            else
            {
               Files.Add(path);
            }

            using (StreamWriter sourceWriter = new(path))
            {
               provider.GenerateCodeFromCompileUnit(
                   TargetUnit, sourceWriter, options);
            }
         }

         return (ReferenzName, new List<string>() { NamespaceName });
      }


      private string GetPath(string basePath)
      {
         var space = NamespaceName.Split('.').ToList();

         space.Add((FileName is null ? ClassName : FileName) + ".cs");

         // Remove the first two parts of the namespace to avoid the files being created in the Ajuna/NetApi sub folder. 
         space = space.TakeLast(space.Count - 2).ToList();

         // Add base path at the beginning of the paths list
         if (!string.IsNullOrEmpty(basePath))
         {
            space.Insert(0, basePath);
         }

         string path = Path.Combine(space.ToArray());

         return path;
      }
   }
}
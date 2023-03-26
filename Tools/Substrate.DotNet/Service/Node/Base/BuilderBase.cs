using Substrate.NetApi.Model.Meta;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Substrate.DotNet.Service.Node.Base
{

   public abstract class BuilderBase
   {
      public static readonly List<string> Files = new();

      public uint Id { get; }
      
      NodeTypeResolver Resolver { get; }

      public bool Success { get; set; }

      public string NamespaceName { get; protected set; }

      internal string FileName { get; set; }

      public string ClassName { get; set; }

      public string ReferenzName { get; set; }

      public string ProjectName { get; private set; }

      public CodeNamespace ImportsNamespace { get; set; }

      public CodeCompileUnit TargetUnit { get; set; }

      public abstract BuilderBase Create();

      public BuilderBase(string projectName, uint id, NodeTypeResolver resolver)
      {
         ProjectName = projectName;
         Id = id;
         Resolver = resolver;
         ImportsNamespace = new()
         {
            Imports = {
               new CodeNamespaceImport("Substrate.NetApi.Model.Types.Base"),
               new CodeNamespaceImport("System.Collections.Generic")
                           }
         };
         TargetUnit = new CodeCompileUnit();
         TargetUnit.Namespaces.Add(ImportsNamespace);

         Success = true;
      }

      public NodeTypeResolved GetFullItemPath(uint typeId)
      {
         if (!Resolver.TypeNames.TryGetValue(typeId, out NodeTypeResolved fullItem))
         {
            Success = false;
            return null;
         }

         return fullItem;
      }

      public static CodeCommentStatementCollection GetComments(string[] docs, NodeType typeDef = null,
          string typeName = null)
      {
         CodeCommentStatementCollection comments = new()
         {
            new CodeCommentStatement("<summary>", true)
         };

         if (typeDef != null)
         {
            string path = typeDef.Path != null ? "[" + string.Join('.', typeDef.Path) + "]" : "";
            comments.Add(new CodeCommentStatement($">> {typeDef.Id} - {typeDef.TypeDef}{path}", true));
         }

         if (typeName != null)
         {
            comments.Add(new CodeCommentStatement($">> {typeName}", true));
         }

         if (docs != null)
         {
            foreach (string doc in docs)
            {
               comments.Add(new CodeCommentStatement(doc, true));
            }
         }

         comments.Add(new CodeCommentStatement("</summary>", true));
         return comments;
      }

      public static CodeMemberMethod SimpleMethod(string name, string returnType = null, object returnExpression = null)
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

      public virtual void Build(bool write, out bool success, string basePath = null)
      {
         success = Success;
         if (write && Success)
         {
            var provider = CodeDomProvider.CreateProvider("CSharp");
            CodeGeneratorOptions options = new()
            {
               BracingStyle = "C"
            };

            string path = GetPath(basePath);

            Directory.CreateDirectory(Path.GetDirectoryName(path));

            if (Files.Contains(path))
            {
               // TODO (svnscha) Why does this happen?
               // Console.WriteLine($"Overwriting[BUG]: {path}");
               //path += _index++;
            }
            else
            {
               Files.Add(path);
            }

            using StreamWriter sourceWriter = new(path);
            provider.GenerateCodeFromCompileUnit(
                TargetUnit, sourceWriter, options);
         }
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

      protected void AddTargetClassCustomAttributes(CodeTypeDeclaration targetClass, NodeType typeDef)
      {
         // TODO (svnscha): Change version to given metadata version.
         ImportsNamespace.Imports.Add(new CodeNamespaceImport("Substrate.NetApi.Model.Types.Metadata.V14"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport($"Substrate.NetApi.Attributes"));

         targetClass.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference("AjunaNodeType"), new CodeAttributeArgument(
            new CodeSnippetExpression($"TypeDefEnum.{typeDef.TypeDef}")
         )));

      }
   }
}
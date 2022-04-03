﻿using Ajuna.NetApi.Model.Meta;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RuntimeMetadata
{
    public abstract class TypeBuilder : BaseBuilder
    {
        public NodeType TypeDef { get; }

        public TypeBuilder(uint id, NodeType typeDef, Dictionary<uint, (string, List<string>)> typeDict)
            : base(id, typeDict)
        {
            TypeDef = typeDef;
            NameSpace = typeDef.Path != null && typeDef.Path[0].Contains("_") ?
                $"{BaseBuilder.BASE_NAMESPACE}.Model.{typeDef.Path[0].MakeMethod()}"
                : $"{BaseBuilder.BASE_NAMESPACE}.Model.Base";
        }
    }

    public abstract class ModuleBuilder : BaseBuilder
    {
        public Dictionary<uint, NodeType> NodeTypes { get; }

        public PalletModule Module { get; }

        public string PrefixName { get; }

        public ModuleBuilder(uint id, PalletModule module, Dictionary<uint, (string, List<string>)> typeDict, Dictionary<uint, NodeType> nodeTypes)
            : base(id, typeDict)
        {
            NodeTypes = nodeTypes;
            Module = module;
            PrefixName = module.Name == "System" ? "Frame" : "Pallet";
            NameSpace = $"{BaseBuilder.BASE_NAMESPACE}.Model.{PrefixName + module.Name.MakeMethod()}";
        }
    }

    public abstract class ModulesBuilder : BaseBuilder
    {
        public Dictionary<uint, NodeType> NodeTypes { get; }

        public PalletModule[] Modules { get; }

        public string PrefixName { get; }

        public ModulesBuilder(uint id, PalletModule[] modules, Dictionary<uint, (string, List<string>)> typeDict, Dictionary<uint, NodeType> nodeTypes)
            : base(id, typeDict)
        {
            NodeTypes = nodeTypes;
            Modules = modules;
        }
    }

    public abstract class ClientBuilder : BaseBuilder
    {
        public List<(string, List<string>)> ModuleNames { get; }

        public ClientBuilder(uint id, List<(string, List<string>)> moduleNames, Dictionary<uint, (string, List<string>)> typeDict)
            : base(id, typeDict)
        {
            ModuleNames = moduleNames;
            NameSpace = BaseBuilder.BASE_NAMESPACE;
        }
    }

    public abstract class BaseBuilder
    {
        public const string BASE_NAMESPACE = "Ajuna.NetApi";

        public static List<string> Files = new();

        public uint Id { get; }

        public Dictionary<uint, (string, List<string>)> TypeDict { get; }

        public bool Success { get; set; }

        public string NameSpace { get; set; }

        internal string FileName { get; set; }


        public string ClassName { get; set; }

        public string ReferenzName { get; set; }

        public CodeNamespace ImportsNamespace { get; set; }

        public CodeCompileUnit TargetUnit { get; set; }

        public abstract BaseBuilder Create();

        public BaseBuilder(uint id, Dictionary<uint, (string, List<string>)> typeDict)
        {
            Id = id;
            TypeDict = typeDict;
            ImportsNamespace = new()
            {
                Imports = {
                    new CodeNamespaceImport("Ajuna.NetApi.Model.Types.Base"),
                    new CodeNamespaceImport("System.Collections.Generic"),
                    new CodeNamespaceImport("System")
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

        public static CodeCommentStatementCollection GetComments(string[] docs, NodeType typeDef = null, string typeName = null)
        {
            CodeCommentStatementCollection comments = new();
            comments.Add(new CodeCommentStatement("<summary>", true));

            if (typeDef != null)
            {
                var path = typeDef.Path != null ? "[" + String.Join('.', typeDef.Path) + "]" : "";
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

        // private static int _index = 1;
        public virtual (string, List<string>) Build(bool write, out bool success)
        {
            success = Success;
            if (write && Success)
            {
                CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
                CodeGeneratorOptions options = new()
                {
                    BracingStyle = "C"
                };
                var space = NameSpace.Split('.').ToList();
                //space.RemoveAt(0);
                space.Add((FileName is null ? ClassName : FileName) + ".cs");
                var path = Path.Combine(space.ToArray());
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
            return (ReferenzName, new List<string>() { NameSpace });
        }
    }
}
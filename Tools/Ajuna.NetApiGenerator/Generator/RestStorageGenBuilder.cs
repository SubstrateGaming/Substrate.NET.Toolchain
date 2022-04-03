﻿using Ajuna.NetApi;
using Ajuna.NetApi.Model.Meta;
using Ajuna.NetApiGenerator;
using Ajuna.NetApiGenerator.Generator;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;

namespace RuntimeMetadata
{
    partial class Program
    {
        public class RestStorageGenBuilder : ModuleBuilder
        {
            private RestStorageGenBuilder(uint id, PalletModule module, Dictionary<uint, (string, List<string>)> typeDict, Dictionary<uint, NodeType> nodeTypes) :
                base(id, module, typeDict, nodeTypes)
            {
            }

            public static RestStorageGenBuilder Init(uint id, PalletModule module, Dictionary<uint, (string, List<string>)> typeDict, Dictionary<uint, NodeType> nodeTypes)
            {
                return new RestStorageGenBuilder(id, module, typeDict, nodeTypes);
            }

            public override RestStorageGenBuilder Create()
            {

                if (Module.Storage == null)
                {
                    Success = false;
                    return this;
                }

                #region CREATE
                ImportsNamespace.Imports.Add(new CodeNamespaceImport("Ajuna.ServiceLayer.Attributes"));
                ImportsNamespace.Imports.Add(new CodeNamespaceImport("Ajuna.ServiceLayer.Storage"));
                ImportsNamespace.Imports.Add(new CodeNamespaceImport("System.Threading.Tasks"));

                FileName = Module.Storage.Prefix + "Storage";

                ReferenzName = "Ajuna.Infrastructure.Storages." + FileName;

                NameSpace = "Ajuna.Infrastructure.Storages";

                CodeNamespace typeNamespace = new(NameSpace);
                TargetUnit.Namespaces.Add(typeNamespace);

                CreateStorage(typeNamespace);

                #endregion

                return this;
            }

            private void CreateStorage(CodeNamespace typeNamespace)
            {
                ClassName = Module.Storage.Prefix + "Storage";

                var targetInterface = new CodeTypeDeclaration($"I{ClassName}")
                {
                    IsInterface = true

                };
                targetInterface.BaseTypes.Add(new CodeTypeReference("IStorage"));
                typeNamespace.Types.Add(targetInterface);


                var targetClass = new CodeTypeDeclaration(ClassName)
                {
                    IsClass = true,
                    TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed
                };
                targetClass.BaseTypes.Add(new CodeTypeReference(targetInterface.Name));

                typeNamespace.Types.Add(targetClass);

                CodeConstructor constructor = new()
                {
                    Attributes = MemberAttributes.Public | MemberAttributes.Final,
                };

                constructor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("IStorageChangeDelegate"), "storageChangeDelegate"));

                targetClass.Members.Add(constructor);

                CodeMemberMethod initializeAsyncMethod = new()
                {
                    Attributes = MemberAttributes.Public | MemberAttributes.Final,
                    Name = $"InitializeAsync",
                    ReturnType = new CodeTypeReference("async Task")

                };
                var clientParamter = new CodeParameterDeclarationExpression(typeof(SubstrateClient), "client");
                initializeAsyncMethod.Parameters.Add(clientParamter);
                targetClass.Members.Add(initializeAsyncMethod);

                if (Module.Storage.Entries != null)
                {
                    var keyParamter = new CodeParameterDeclarationExpression(typeof(System.String), "key");
                    var dataParamter = new CodeParameterDeclarationExpression(typeof(System.String), "data");

                    foreach (var entry in Module.Storage.Entries)
                    {
                        CodeTypeReference baseReturnType;
                        CodeTypeReference returnType;
                        CodeExpression[] updateExpression, tryGetExpression;
                        if (entry.StorageType == Ajuna.NetApi.Model.Meta.Storage.Type.Plain)
                        {
                            var fullItem = GetFullItemPath(entry.TypeMap.Item1);
                            baseReturnType = new CodeTypeReference(fullItem.Item1);
                            returnType = new CodeTypeReference($"TypedStorage<{fullItem.Item1}>");

                            updateExpression = new CodeExpression[] {
                                            new CodeVariableReferenceExpression(dataParamter.Name)};
                            tryGetExpression = Array.Empty<CodeExpression>();
                        }
                        else if (entry.StorageType == Ajuna.NetApi.Model.Meta.Storage.Type.Map)
                        {
                            var typeMap = entry.TypeMap.Item2;
                            var hashers = typeMap.Hashers;
                            var key = GetFullItemPath(typeMap.Key);
                            var value = GetFullItemPath(typeMap.Value);
                            baseReturnType = new CodeTypeReference(value.Item1);
                            returnType = new CodeTypeReference($"TypedMapStorage<{value.Item1}>");

                            updateExpression = new CodeExpression[] {
                                new CodeVariableReferenceExpression(keyParamter.Name),
                                new CodeVariableReferenceExpression(dataParamter.Name)};
                            tryGetExpression = new CodeExpression[] {
                                new CodeVariableReferenceExpression(keyParamter.Name),
                                new CodeParameterDeclarationExpression(baseReturnType, "result") {
                                    Direction = FieldDirection.Out
                                }
                            };
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }

                        // create typed storage field
                        CodeMemberField field = new()
                        {
                            Attributes = MemberAttributes.Private,
                            Name = $"{entry.Name.MakePrivateField()}TypedStorage",
                            Type = returnType
                        };
                        targetClass.Members.Add(field);

                        // create typed storage property
                        CodeMemberProperty prop = new()
                        {
                            Attributes = MemberAttributes.Public | MemberAttributes.Final,
                            Name = field.Name.MakeMethod(),
                            HasGet = true,
                            Type = field.Type
                        };
                        prop.GetStatements.Add(new CodeMethodReturnStatement(
                            new CodeVariableReferenceExpression(field.Name)));
                        prop.SetStatements.Add(new CodeAssignStatement(
                            new CodeVariableReferenceExpression(field.Name),
                                new CodePropertySetValueReferenceExpression()));
                        targetClass.Members.Add(prop);

                        // constructor initialize storage properties
                        constructor.Statements.Add(new CodeAssignStatement(
                            new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), prop.Name),
                        new CodeObjectCreateExpression(field.Type,
                            new CodeExpression[] {
                                new CodePrimitiveExpression($"{Module.Storage.Prefix}.{entry.Name}"),
                                new CodeVariableReferenceExpression("storageChangeDelegate")
                            })));

                        // create initialize records foreach storage
                        CodeMethodInvokeExpression initializeAsyncInvoke = new(
                            new CodeVariableReferenceExpression($"await {prop.Name}"),
                            "InitializeAsync", new CodeExpression[] {
                                new CodeVariableReferenceExpression("client"),
                                new CodePrimitiveExpression(Module.Storage.Prefix),
                                new CodePrimitiveExpression(entry.Name)
                        });
                        initializeAsyncMethod.Statements.Add(initializeAsyncInvoke);

                        // create on update
                        CodeMemberMethod onUpdateMethod = new()
                        {
                            Attributes = MemberAttributes.Public | MemberAttributes.Final,
                            Name = $"OnUpdate{entry.Name}",
                        };

                        onUpdateMethod.CustomAttributes.Add(
                            new CodeAttributeDeclaration("StorageChange",
                            new CodeAttributeArgument[] {
                                new CodeAttributeArgument(new CodePrimitiveExpression(Module.Storage.Prefix)),
                                new CodeAttributeArgument(new CodePrimitiveExpression(entry.Name))
                                }));

                        CodeMethodInvokeExpression updateInvoke = new(
                            new CodeVariableReferenceExpression(prop.Name),
                            "Update", updateExpression);
                        onUpdateMethod.Statements.Add(updateInvoke);
                        targetClass.Members.Add(onUpdateMethod);

                        // create get and gets
                        CodeMemberMethod getStorageMethod = new()
                        {
                            Attributes = MemberAttributes.Public | MemberAttributes.Final,
                            Name = $"Get{entry.Name}",
                            ReturnType = baseReturnType
                        };
                        getStorageMethod.Comments.AddRange(GetComments(entry.Docs, null, entry.Name));

                        targetInterface.Members.Add(getStorageMethod);

                        if (tryGetExpression.Length == 0)
                        {
                            onUpdateMethod.Parameters.Add(dataParamter);

                            getStorageMethod.Statements.Add(new CodeMethodReturnStatement(
                                    new CodeMethodInvokeExpression(
                                        new CodeVariableReferenceExpression(prop.Name),
                                        "Get", Array.Empty<CodeExpression>())));
                        }
                        else
                        {
                            onUpdateMethod.Parameters.Add(keyParamter);
                            onUpdateMethod.Parameters.Add(dataParamter);

                            getStorageMethod.Parameters.Add(keyParamter);

                            getStorageMethod.Statements.Add(new CodeConditionStatement(
                               new CodeBinaryOperatorExpression(
                                   new CodeVariableReferenceExpression("key"),
                                   CodeBinaryOperatorType.ValueEquality,
                                   new CodePrimitiveExpression(null)),
                               new CodeStatement[] { new CodeMethodReturnStatement(new CodePrimitiveExpression(null)) }));

                            getStorageMethod.Statements.Add(new CodeConditionStatement(
                                new CodeMethodInvokeExpression(
                                    new CodeVariableReferenceExpression(prop.Name),
                                        "Dictionary.TryGetValue", tryGetExpression),
                                new CodeStatement[] { new CodeMethodReturnStatement(new CodeVariableReferenceExpression("result")) },
                                new CodeStatement[] { new CodeMethodReturnStatement(new CodePrimitiveExpression(null)) }));
                        }

                        targetClass.Members.Add(getStorageMethod);


                    }
                }
            }

        }

    }

}

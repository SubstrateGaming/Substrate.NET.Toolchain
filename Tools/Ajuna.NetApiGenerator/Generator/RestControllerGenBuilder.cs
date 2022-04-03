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
        public class RestControllerGenBuilder : ModuleBuilder
        {
            private RestControllerGenBuilder(uint id, PalletModule module, Dictionary<uint, (string, List<string>)> typeDict, Dictionary<uint, NodeType> nodeTypes) :
                base(id, module, typeDict, nodeTypes)
            {
            }

            public static RestControllerGenBuilder Init(uint id, PalletModule module, Dictionary<uint, (string, List<string>)> typeDict, Dictionary<uint, NodeType> nodeTypes)
            {
                return new RestControllerGenBuilder(id, module, typeDict, nodeTypes);
            }

            public override RestControllerGenBuilder Create()
            {

                if (Module.Storage == null)
                {
                    Success = false;
                    return this;
                }

                #region CREATE
                ImportsNamespace.Imports.Add(new CodeNamespaceImport("Ajuna.Infrastructure.Storages"));
                ImportsNamespace.Imports.Add(new CodeNamespaceImport("Microsoft.AspNetCore.Mvc"));
                ImportsNamespace.Imports.Add(new CodeNamespaceImport("System.Threading.Tasks"));

                FileName = Module.Storage.Prefix + "Controller";

                ReferenzName = "Ajuna.Infrastructure.RestService.Controller." + FileName;

                NameSpace = "Ajuna.Infrastructure.RestService.Controller";

                CodeNamespace typeNamespace = new(NameSpace);
                TargetUnit.Namespaces.Add(typeNamespace);

                CreateController(typeNamespace);

                #endregion

                return this;
            }

            private void CreateController(CodeNamespace typeNamespace)
            {
                ClassName = FileName;

                var targetClass = new CodeTypeDeclaration(ClassName)
                {
                    IsClass = true,
                    TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed

                };
                targetClass.BaseTypes.Add(new CodeTypeReference("ControllerBase"));
                targetClass.Comments.AddRange(GetComments(new string[] { $"{ClassName} controller to access storages." }));


                typeNamespace.Types.Add(targetClass);
                targetClass.CustomAttributes.Add(
                    new CodeAttributeDeclaration("ApiController"));
                targetClass.CustomAttributes.Add(
                    new CodeAttributeDeclaration("Route",
                    new CodeAttributeArgument[] {
                        new CodeAttributeArgument(new CodePrimitiveExpression("[controller]"))
                    }));

                var fieldName = $"{Module.Storage.Prefix}Storage";
                CodeMemberField field = new()
                {
                    Attributes = MemberAttributes.Private,
                    Name = fieldName.MakePrivateField(),
                    Type = new CodeTypeReference($"I{fieldName}")
                };
                targetClass.Members.Add(field);

                CodeConstructor constructor = new()
                {
                    Attributes =
                    MemberAttributes.Public | MemberAttributes.Final
                };
                targetClass.Members.Add(constructor);
                constructor.Comments.AddRange(GetComments(new string[] { $"{ClassName} constructor." }));

                constructor.Parameters.Add(new CodeParameterDeclarationExpression($"I{fieldName}", fieldName.MakePublicField()));

                // constructor initialize storage properties
                constructor.Statements.Add(new CodeAssignStatement(
                    new CodeVariableReferenceExpression(field.Name),
                    new CodeVariableReferenceExpression(fieldName.MakePublicField())));


                if (Module.Storage.Entries != null)
                {
                    foreach (var entry in Module.Storage.Entries)
                    {
                        CodeParameterDeclarationExpression parameterDeclaration;
                        CodeTypeReference baseReturnType;
                        CodeExpression[] codeExpressions;
                        if (entry.StorageType == Ajuna.NetApi.Model.Meta.Storage.Type.Plain)
                        {
                            var fullItem = GetFullItemPath(entry.TypeMap.Item1);
                            baseReturnType = new CodeTypeReference(fullItem.Item1);
                            parameterDeclaration = null;
                            codeExpressions = Array.Empty<CodeExpression>();
                        }
                        else if (entry.StorageType == Ajuna.NetApi.Model.Meta.Storage.Type.Map)
                        {
                            var typeMap = entry.TypeMap.Item2;
                            var hashers = typeMap.Hashers;
                            var key = GetFullItemPath(typeMap.Key);
                            var value = GetFullItemPath(typeMap.Value);
                            baseReturnType = new CodeTypeReference(value.Item1);
                            parameterDeclaration = new CodeParameterDeclarationExpression(typeof(System.String), "key");
                            codeExpressions = new CodeExpression[] {
                                new CodeVariableReferenceExpression(parameterDeclaration.Name)
                            };
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }

                        // create get and gets
                        CodeMemberMethod getStorageMethod = new()
                        {
                            Attributes = MemberAttributes.Public | MemberAttributes.Final,
                            Name = $"Get{entry.Name}",
                            ReturnType = new CodeTypeReference("IActionResult")
                        };
                        getStorageMethod.Comments.AddRange(GetComments(entry.Docs, null, entry.Name));
                        getStorageMethod.CustomAttributes.Add(
                            new CodeAttributeDeclaration("HttpGet",
                            new CodeAttributeArgument[] {
                                new CodeAttributeArgument(new CodePrimitiveExpression(entry.Name))
                            })
                        );
                        getStorageMethod.CustomAttributes.Add(
                            new CodeAttributeDeclaration("ProducesResponseType",
                            new CodeAttributeArgument[] {
                                new CodeAttributeArgument(new CodeTypeOfExpression(baseReturnType)),
                                new CodeAttributeArgument(new CodePrimitiveExpression(200))
                            }));

                        if (parameterDeclaration != null)
                        {
                            getStorageMethod.Parameters.Add(parameterDeclaration);
                        }

                        getStorageMethod.Statements.Add(
                            new CodeMethodReturnStatement(new CodeMethodInvokeExpression(
                                new CodeThisReferenceExpression(),
                                "Ok",
                                new CodeExpression[] { new CodeMethodInvokeExpression(
                                    new CodeVariableReferenceExpression(field.Name),
                                    getStorageMethod.Name,
                                    codeExpressions
                                    )}
                                )));

                        targetClass.Members.Add(getStorageMethod);


                    }
                }
            }

        }

    }

}

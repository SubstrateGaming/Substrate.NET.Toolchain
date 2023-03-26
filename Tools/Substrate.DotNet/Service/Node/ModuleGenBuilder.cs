using Substrate.DotNet.Extensions;
using Substrate.DotNet.Service.Node.Base;
using Substrate.NetApi.Model.Extrinsics;
using Substrate.NetApi.Model.Meta;
using Substrate.NetApi.Model.Types;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Substrate.DotNet.Service.Node
{
   public class ModuleGenBuilder : ModuleBuilderBase
   {
      private ModuleGenBuilder(string projectName, uint id, PalletModule module, NodeTypeResolver typeDict, Dictionary<uint, NodeType> nodeTypes) :
          base(projectName, id, module, typeDict, nodeTypes)
      {
      }

      public static ModuleGenBuilder Init(string projectName, uint id, PalletModule module, NodeTypeResolver typeDict, Dictionary<uint, NodeType> nodeTypes)
      {
         return new ModuleGenBuilder(projectName, id, module, typeDict, nodeTypes);
      }

      public override ModuleGenBuilder Create()
      {
         ImportsNamespace.Imports.Add(new CodeNamespaceImport("System.Threading.Tasks"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport($"Substrate.NetApi.Model.Meta"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport("System.Threading"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport($"Substrate.NetApi"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport($"Substrate.NetApi.Model.Types"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport($"Substrate.NetApi.Model.Extrinsics"));

         FileName = "Main" + Module.Name;
         NamespaceName = $"{ProjectName}.Generated.Storage";
         ReferenzName = NamespaceName;

         CodeNamespace typeNamespace = new(NamespaceName);
         TargetUnit.Namespaces.Add(typeNamespace);

         // add constructor
         CodeConstructor constructor = new()
         {
            Attributes = MemberAttributes.Public | MemberAttributes.Final
         };

         CreateStorage(typeNamespace, constructor);
         CreateCalls(typeNamespace);
         CreateEvents(typeNamespace);
         CreateConstants(typeNamespace);
         CreateErrors(typeNamespace);

         return this;
      }

      private void CreateStorage(CodeNamespace typeNamespace, CodeConstructor constructor)
      {
         ClassName = Module.Name + "Storage";

         PalletStorage storage = Module.Storage;

         var targetClass = new CodeTypeDeclaration(ClassName)
         {
            IsClass = true,
            TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed
         };
         typeNamespace.Types.Add(targetClass);

         // Declare the client field.
         var clientField = new CodeMemberField
         {
            Attributes = MemberAttributes.Private,
            Name = "_client",
            Type = new CodeTypeReference("SubstrateClientExt")
         };
         clientField.Comments.Add(new CodeCommentStatement("Substrate client for the storage calls."));
         targetClass.Members.Add(clientField);

         // Add parameters.
         constructor.Parameters.Add(new CodeParameterDeclarationExpression(
             clientField.Type, "client"));
         CodeFieldReferenceExpression fieldReference =
             new(new CodeThisReferenceExpression(), "_client");
         constructor.Statements.Add(new CodeAssignStatement(fieldReference,
             new CodeArgumentReferenceExpression("client")));

         targetClass.Members.Add(constructor);

         if (storage?.Entries != null)
         {
            foreach (Entry entry in storage.Entries)
            {
               string storageParams = entry.Name + "Params";
               CodeMemberMethod parameterMethod = new()
               {
                  Attributes = MemberAttributes.Static | MemberAttributes.Public | MemberAttributes.Final,
                  Name = storageParams,
                  ReturnType = new CodeTypeReference(typeof(string))
               };
               // add comment to class if exists
               parameterMethod.Comments.AddRange(GetComments(entry.Docs, null, storageParams));
               targetClass.Members.Add(parameterMethod);

               // default function
               if (entry.Default != null || entry.Default.Length != 0)
               {
                  string storageDefault = entry.Name + "Default";
                  CodeMemberMethod defaultMethod = new()
                  {
                     Attributes = MemberAttributes.Static | MemberAttributes.Public | MemberAttributes.Final,
                     Name = storageDefault,
                     ReturnType = new CodeTypeReference(typeof(string))
                  };
                  // add comment to class if exists
                  defaultMethod.Comments.AddRange(GetComments(new string[] { "Default value as hex string" }, null, storageDefault));
                  targetClass.Members.Add(defaultMethod);
                  // add return statement
                  defaultMethod.Statements.Add(new CodeMethodReturnStatement(
                     new CodePrimitiveExpression("0x" + BitConverter.ToString(entry.Default).Replace("-", string.Empty))));
               }

               // async Task<object>
               CodeMemberMethod storageMethod = new()
               {
                  Attributes = MemberAttributes.Public | MemberAttributes.Final,
                  Name = entry.Name,
               };
               // add comment to class if exists
               storageMethod.Comments.AddRange(GetComments(entry.Docs, null, entry.Name));

               targetClass.Members.Add(storageMethod);

               if (entry.StorageType == Storage.Type.Plain)
               {
                  NodeTypeResolved fullItem = GetFullItemPath(entry.TypeMap.Item1);

                  parameterMethod.Statements.Add(new CodeMethodReturnStatement(
                      ModuleGenBuilder.GetStorageString(storage.Prefix, entry.Name, entry.StorageType)));

                  storageMethod.ReturnType = new CodeTypeReference($"async Task<{fullItem}>");

                  storageMethod.Parameters.Add(new CodeParameterDeclarationExpression("CancellationToken", "token"));

                  CodeMethodInvokeExpression methodInvoke = new(
                      new CodeTypeReferenceExpression(targetClass.Name),
                      parameterMethod.Name, Array.Empty<CodeExpression>());

                  CodeVariableDeclarationStatement variableDeclaration1 = new(typeof(string), "parameters", methodInvoke);
                  storageMethod.Statements.Add(variableDeclaration1);

                  // create result
                  var resultStatement = new CodeArgumentReferenceExpression(ModuleGenBuilder.GetInvoceString(fullItem.ToString()));
                  CodeVariableDeclarationStatement variableDeclaration2 = new("var", "result", resultStatement);
                  storageMethod.Statements.Add(variableDeclaration2);

                  // return statement
                  storageMethod.Statements.Add(
                     new CodeMethodReturnStatement(
                        new CodeVariableReferenceExpression("result")));

                  // add storage key mapping in constructor
                  constructor.Statements.Add(
                      ModuleGenBuilder.AddPropertyValues(ModuleGenBuilder.GetStorageMapString("", fullItem.ToString(), storage.Prefix, entry.Name), "_client.StorageKeyDict"));
               }
               else if (entry.StorageType == Storage.Type.Map)
               {
                  TypeMap typeMap = entry.TypeMap.Item2;
                  Storage.Hasher[] hashers = typeMap.Hashers;
                  NodeTypeResolved key = GetFullItemPath(typeMap.Key);
                  NodeTypeResolved value = GetFullItemPath(typeMap.Value);

                  parameterMethod.Parameters.Add(new CodeParameterDeclarationExpression(key.ToString(), "key"));
                  parameterMethod.Statements.Add(new CodeMethodReturnStatement(
                      ModuleGenBuilder.GetStorageString(storage.Prefix, entry.Name, entry.StorageType, hashers)));

                  storageMethod.ReturnType = new CodeTypeReference($"async Task<{value}>");
                  storageMethod.Parameters.Add(new CodeParameterDeclarationExpression(key.ToString(), "key"));
                  storageMethod.Parameters.Add(new CodeParameterDeclarationExpression("CancellationToken", "token"));

                  CodeMethodInvokeExpression methodInvoke = new(new CodeTypeReferenceExpression(targetClass.Name), parameterMethod.Name,
                      new CodeExpression[] { new CodeArgumentReferenceExpression("key") });
                  CodeVariableDeclarationStatement variableDeclaration = new(typeof(string), "parameters", methodInvoke);
                  storageMethod.Statements.Add(variableDeclaration);

                  // create result
                  var resultStatement = new CodeArgumentReferenceExpression(ModuleGenBuilder.GetInvoceString(value.ToString()));
                  CodeVariableDeclarationStatement variableDeclaration2 = new("var", "result", resultStatement);
                  storageMethod.Statements.Add(variableDeclaration2);

                  // default handling
                  //if (entry.Default != null || entry.Default.Length != 0)
                  //{
                  //   var conditionalStatement = new CodeConditionStatement(
                  //    new CodeBinaryOperatorExpression(
                  //      new CodeVariableReferenceExpression("result"),
                  //      CodeBinaryOperatorType.ValueEquality,
                  //      new CodePrimitiveExpression(null)),
                  //    new CodeStatement[] {
                  //      new CodeAssignStatement(new CodeVariableReferenceExpression("result"), new CodeObjectCreateExpression( value.ToString(), Array.Empty<CodeExpression>() )),
                  //      new CodeExpressionStatement(
                  //          new CodeMethodInvokeExpression(
                  //            new CodeVariableReferenceExpression("result"), "Create",
                  //            new CodeExpression[] { new CodePrimitiveExpression("0x" + BitConverter.ToString(entry.Default).Replace("-", string.Empty)) }))});
                  //   storageMethod.Statements.Add(conditionalStatement);
                  //}

                  // return statement
                  storageMethod.Statements.Add(
                      new CodeMethodReturnStatement(
                          new CodeVariableReferenceExpression("result")));

                  // add storage key mapping in constructor
                  constructor.Statements.Add(ModuleGenBuilder.AddPropertyValues(ModuleGenBuilder.GetStorageMapString(key.ToString(), value.ToString(), storage.Prefix, entry.Name, hashers), "_client.StorageKeyDict"));
               }
               else
               {
                  throw new NotImplementedException();
               }
            }
         }
      }

      private void CreateCalls(CodeNamespace typeNamespace)
      {
         ClassName = Module.Name + "Calls";

         PalletCalls calls = Module.Calls;

         var targetClass = new CodeTypeDeclaration(ClassName)
         {
            IsClass = true,
            TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed
         };
         typeNamespace.Types.Add(targetClass);

         if (calls != null)
         {
            if (NodeTypes.TryGetValue(calls.TypeId, out NodeType nodeType))
            {
               var typeDef = nodeType as NodeTypeVariant;

               if (typeDef.Variants != null)
               {
                  foreach (TypeVariant variant in typeDef.Variants)
                  {
                     CodeMemberMethod callMethod = new()
                     {
                        Attributes = MemberAttributes.Static | MemberAttributes.Public | MemberAttributes.Final,
                        Name = variant.Name.MakeMethod(),
                        ReturnType = new CodeTypeReference(typeof(Method).Name)
                     };

                     // add comment to class if exists
                     callMethod.Comments.AddRange(GetComments(typeDef.Docs, null, variant.Name));

                     string byteArrayName = "byteArray";

                     callMethod.Statements.Add(new CodeVariableDeclarationStatement(
                         typeof(List<byte>), byteArrayName, new CodeObjectCreateExpression("List<byte>", Array.Empty<CodeExpression>())));

                     if (variant.TypeFields != null)
                     {
                        foreach (NodeTypeField field in variant.TypeFields)
                        {
                           NodeTypeResolved fullItem = GetFullItemPath(field.TypeId);

                           CodeParameterDeclarationExpression param = new()
                           {
                              Type = new CodeTypeReference(fullItem.ToString()),
                              Name = field.Name
                           };
                           callMethod.Parameters.Add(param);

                           callMethod.Statements.Add(new CodeMethodInvokeExpression(
                               new CodeVariableReferenceExpression(byteArrayName), "AddRange", new CodeMethodInvokeExpression(
                               new CodeVariableReferenceExpression(field.Name), "Encode")));
                        }
                     }

                     // return statment
                     var create = new CodeObjectCreateExpression(typeof(Method).Name, Array.Empty<CodeExpression>());
                     create.Parameters.Add(new CodePrimitiveExpression((int)Module.Index));
                     create.Parameters.Add(new CodePrimitiveExpression(Module.Name));
                     create.Parameters.Add(new CodePrimitiveExpression(variant.Index));
                     create.Parameters.Add(new CodePrimitiveExpression(variant.Name));
                     create.Parameters.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(byteArrayName), "ToArray"));
                     CodeMethodReturnStatement returnStatement = new()
                     {
                        Expression = create
                     };

                     callMethod.Statements.Add(returnStatement);
                     targetClass.Members.Add(callMethod);
                  }
               }
            }
         }
      }

      private void CreateEvents(CodeNamespace typeNamespace)
      {
         ClassName = Module.Name + "Events";

         PalletEvents events = Module.Events;

         //if (events != null)
         //{
         //   if (NodeTypes.TryGetValue(events.TypeId, out NodeType nodeType))
         //   {
         //      var typeDef = nodeType as NodeTypeVariant;

         //      if (typeDef.Variants != null)
         //      {
         //         foreach (TypeVariant variant in typeDef.Variants)
         //         {
         //            var eventClass = new CodeTypeDeclaration("Event" + variant.Name.MakeMethod())
         //            {
         //               IsClass = true,
         //               TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed
         //            };

         //            // add comment to variant if exists
         //            eventClass.Comments.AddRange(GetComments(variant.Docs, null, variant.Name));

         //            var codeTypeRef = new CodeTypeReference("BaseTuple");
         //            if (variant.TypeFields != null)
         //            {
         //               foreach (NodeTypeField field in variant.TypeFields)
         //               {
         //                  NodeTypeResolved fullItem = GetFullItemPath(field.TypeId);
         //                  codeTypeRef.TypeArguments.Add(new CodeTypeReference(fullItem.ToString()));
         //               }
         //            }
         //            eventClass.BaseTypes.Add(codeTypeRef);

         //            // add event key mapping in constructor
         //            // TODO (svnscha) What is with events?
         //            //Console.WriteLine($"case \"{Module.Index}-{variant.Index}\": return typeof({NamespaceName + "." + eventClass.Name});");
         //            //constructor.Statements.Add(
         //            //    AddPropertyValues(new CodeExpression[] {
         //            //     new CodeObjectCreateExpression(
         //            //        new CodeTypeReference(typeof(Tuple<int, int>)),
         //            //        new CodeExpression[] {
         //            //            new CodePrimitiveExpression((int) Module.Index),
         //            //            new CodePrimitiveExpression((int) variant.Index)
         //            //        }),
         //            //     new CodeTypeOfExpression(NameSpace + "." + eventClass.Name)

         //            //    }, "SubstrateClientExt.EventKeyDict"));

         //            typeNamespace.Types.Add(eventClass);
         //         }
         //      }
         //   }
         //}
      }

      private void CreateConstants(CodeNamespace typeNamespace)
      {
         ClassName = Module.Name + "Constants";

         PalletConstant[] constants = Module.Constants;

         var targetClass = new CodeTypeDeclaration(ClassName)
         {
            IsClass = true,
            TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed
         };
         typeNamespace.Types.Add(targetClass);

         if (constants != null && constants.Any())
         {
            foreach (PalletConstant constant in constants)
            {
               // async Task<object>
               CodeMemberMethod constantMethod = new()
               {
                  Attributes = MemberAttributes.Public | MemberAttributes.Final,
                  Name = constant.Name,
               };
               // add comment to class if exists
               constantMethod.Comments.AddRange(GetComments(constant.Docs, null, constant.Name));

               targetClass.Members.Add(constantMethod);

               if (NodeTypes.TryGetValue(constant.TypeId, out NodeType nodeType))
               {
                  NodeTypeResolved nodeTypeResolved = GetFullItemPath(nodeType.Id);
                  constantMethod.ReturnType = new CodeTypeReference(nodeTypeResolved.ToString());

                  // assign new result object
                  CodeVariableDeclarationStatement newStatement = new("var", "result", new CodeObjectCreateExpression(nodeTypeResolved.ToString(), Array.Empty<CodeExpression>()));
                  constantMethod.Statements.Add(newStatement);

                  // create with hex string object
                  var createStatement = new CodeExpressionStatement(
                            new CodeMethodInvokeExpression(
                              new CodeVariableReferenceExpression("result"), "Create",
                              new CodeExpression[] { new CodePrimitiveExpression("0x" + BitConverter.ToString(constant.Value).Replace("-", string.Empty)) }));
                  constantMethod.Statements.Add(createStatement);

                  // return statement
                  constantMethod.Statements.Add(
                      new CodeMethodReturnStatement(
                          new CodeVariableReferenceExpression("result")));
               }
            }
         }
      }

      private void CreateErrors(CodeNamespace typeNamespace)
      {
         ClassName = Module.Name + "Errors";

         PalletErrors errors = Module.Errors;

         if (errors != null)
         {
            if (NodeTypes.TryGetValue(errors.TypeId, out NodeType nodeType))
            {
               var typeDef = nodeType as NodeTypeVariant;

               var targetClass = new CodeTypeDeclaration(ClassName)
               {
                  IsEnum = true,
                  TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed
               };

               if (typeDef.Variants != null)
               {
                  foreach (TypeVariant variant in typeDef.Variants)
                  {
                     var enumField = new CodeMemberField(ClassName, variant.Name);

                     // add comment to field if exists
                     enumField.Comments.AddRange(GetComments(variant.Docs, null, variant.Name));

                     targetClass.Members.Add(enumField);
                  }
               }

               typeNamespace.Types.Add(targetClass);
            }
         }
      }

      private static string GetInvoceString(string returnType)
      {
         return "await _client.GetStorageAsync<" + returnType + ">(parameters, token)";
      }

      private static CodeMethodInvokeExpression GetStorageString(string module, string item, Storage.Type type, Storage.Hasher[] hashers = null)
      {
         var codeExpressions =
             new CodeExpression[] {
                        new CodePrimitiveExpression(module),
                        new CodePrimitiveExpression(item),
                        new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(typeof(Storage.Type)), type.ToString())
          };

         // if it is a map fill hashers and key
         if (hashers != null && hashers.Length > 0)
         {
            CodeExpression keyReference = new CodeArrayCreateExpression(
               new CodeTypeReference(typeof(IType)),
               new CodeArgumentReferenceExpression[] {
                  new CodeArgumentReferenceExpression("key")
            });

            if (hashers.Length > 1)
            {
               keyReference = new CodeSnippetExpression("key.Value");
            }

            codeExpressions = new CodeExpression[] {
                        new CodePrimitiveExpression(module),
                        new CodePrimitiveExpression(item),
                        new CodePropertyReferenceExpression(
                            new CodeTypeReferenceExpression(typeof(Storage.Type)), type.ToString()),
                        new CodeArrayCreateExpression(
                            new CodeTypeReference(typeof(Storage.Hasher)),
                                hashers.Select(p => new CodePropertyReferenceExpression(
                                    new CodeTypeReferenceExpression(typeof(Storage.Hasher)), p.ToString())).ToArray()),
                        keyReference
                    };
         }

         return new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("RequestGenerator"), "GetStorage", codeExpressions);
      }

      private static CodeExpression[] GetStorageMapString(string keyType, string returnType, string module, string item, Storage.Hasher[] hashers = null)
      {
         var typeofReturn = new CodeTypeOfExpression(returnType);

         var result = new CodeExpression[] {
                    new CodeObjectCreateExpression(
                            new CodeTypeReference(typeof(Tuple<string,string>)),
                            new CodeExpression[] {
                                new CodePrimitiveExpression(module),
                                new CodePrimitiveExpression(item)
                            }),
                    new CodeObjectCreateExpression(
                        new CodeTypeReference(typeof(Tuple<Storage.Hasher[], Type, Type>)),
                        new CodeExpression[] {
                            new CodePrimitiveExpression(null),
                            new CodePrimitiveExpression(null),
                            typeofReturn})
                };

         // if it is a map fill hashers and key
         if (hashers != null && hashers.Length > 0)
         {
            var arrayExpression = new CodeArrayCreateExpression(
                        new CodeTypeReference(typeof(Storage.Hasher)),
                            hashers.Select(p => new CodePropertyReferenceExpression(
                                new CodeTypeReferenceExpression(typeof(Storage.Hasher)), p.ToString())).ToArray());
            var typeofType = new CodeTypeOfExpression(keyType);

            result = new CodeExpression[] {
                            new CodeObjectCreateExpression(
                                new CodeTypeReference(typeof(Tuple<string,string>)),
                                new CodeExpression[] {
                                    new CodePrimitiveExpression(module),
                                    new CodePrimitiveExpression(item)
                            }),
                        new CodeObjectCreateExpression(
                            new CodeTypeReference(typeof(Tuple<Storage.Hasher[], Type, Type>)),
                            new CodeExpression[] {
                                arrayExpression,
                                typeofType,
                                typeofReturn
                            })
                    };
         }

         return result;
      }

      private static CodeStatement AddPropertyValues(CodeExpression[] exprs, string variableReference)
      {
         return new CodeExpressionStatement(
             new CodeMethodInvokeExpression(
                 new CodeMethodReferenceExpression(
                     new CodeTypeReferenceExpression(
                         new CodeTypeReference(variableReference)), "Add"), exprs));
      }
   }
}
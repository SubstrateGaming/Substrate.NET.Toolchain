using Ajuna.DotNet.Extensions;
using Ajuna.DotNet.Service.Node.Base;
using Ajuna.NetApi.Model.Extrinsics;
using Ajuna.NetApi.Model.Meta;
using Ajuna.NetApi.Model.Types;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ajuna.DotNet.Service.Node
{
   public class ModuleGenBuilder : ModuleBuilderBase
   {
      private ModuleGenBuilder(string projectName, uint id, PalletModule module, Dictionary<uint, (string, List<string>)> typeDict, Dictionary<uint, NodeType> nodeTypes) :
          base(projectName, id, module, typeDict, nodeTypes)
      {
      }

      public static ModuleGenBuilder Init(string projectName, uint id, PalletModule module, Dictionary<uint, (string, List<string>)> typeDict, Dictionary<uint, NodeType> nodeTypes)
      {
         return new ModuleGenBuilder(projectName, id, module, typeDict, nodeTypes);
      }

      public override ModuleGenBuilder Create()
      {
         ImportsNamespace.Imports.Add(new CodeNamespaceImport("System.Threading.Tasks"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport($"Ajuna.NetApi.Model.Meta"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport("System.Threading"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport($"Ajuna.NetApi"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport($"Ajuna.NetApi.Model.Types"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport($"Ajuna.NetApi.Model.Extrinsics"));

         FileName = "Main" + Module.Name;
         ReferenzName = $"{ProjectName}.Model.{Module.Name}";

         CodeNamespace typeNamespace = new(NamespaceName);
         TargetUnit.Namespaces.Add(typeNamespace);

         // add constructor
         CodeConstructor constructor = new()
         {
            Attributes = MemberAttributes.Public | MemberAttributes.Final
         };

         CreateStorage(typeNamespace, constructor);
         CreateCalls(typeNamespace);

         // CreateEvents(typeNamespace, constructor);
         // CreateConstants(typeNamespace, constructor);

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
                  (string, List<string>) fullItem = GetFullItemPath(entry.TypeMap.Item1);

                  parameterMethod.Statements.Add(new CodeMethodReturnStatement(
                      ModuleGenBuilder.GetStorageString(storage.Prefix, entry.Name, entry.StorageType)));

                  storageMethod.ReturnType = new CodeTypeReference($"async Task<{fullItem.Item1}>");

                  storageMethod.Parameters.Add(new CodeParameterDeclarationExpression("CancellationToken", "token"));

                  CodeMethodInvokeExpression methodInvoke = new(
                      new CodeTypeReferenceExpression(targetClass.Name),
                      parameterMethod.Name, Array.Empty<CodeExpression>());

                  CodeVariableDeclarationStatement variableDeclaration = new(typeof(string), "parameters", methodInvoke);

                  storageMethod.Statements.Add(variableDeclaration);

                  storageMethod.Statements.Add(new CodeMethodReturnStatement(new CodeArgumentReferenceExpression(ModuleGenBuilder.GetInvoceString(fullItem.Item1))));

                  // add storage key mapping in constructor
                  constructor.Statements.Add(
                      ModuleGenBuilder.AddPropertyValues(ModuleGenBuilder.GetStorageMapString("", fullItem.Item1, storage.Prefix, entry.Name), "_client.StorageKeyDict"));

               }
               else if (entry.StorageType == Storage.Type.Map)
               {
                  TypeMap typeMap = entry.TypeMap.Item2;
                  Storage.Hasher[] hashers = typeMap.Hashers;
                  (string, List<string>) key = GetFullItemPath(typeMap.Key);
                  (string, List<string>) value = GetFullItemPath(typeMap.Value);

                  parameterMethod.Parameters.Add(new CodeParameterDeclarationExpression(key.Item1, "key"));
                  parameterMethod.Statements.Add(new CodeMethodReturnStatement(
                      ModuleGenBuilder.GetStorageString(storage.Prefix, entry.Name, entry.StorageType, hashers)));

                  storageMethod.ReturnType = new CodeTypeReference($"async Task<{value.Item1}>");
                  storageMethod.Parameters.Add(new CodeParameterDeclarationExpression(key.Item1, "key"));
                  storageMethod.Parameters.Add(new CodeParameterDeclarationExpression("CancellationToken", "token"));

                  CodeMethodInvokeExpression methodInvoke = new(new CodeTypeReferenceExpression(targetClass.Name), parameterMethod.Name,
                      new CodeExpression[] { new CodeArgumentReferenceExpression("key") });
                  CodeVariableDeclarationStatement variableDeclaration = new(typeof(string), "parameters", methodInvoke);
                  storageMethod.Statements.Add(variableDeclaration);

                  storageMethod.Statements.Add(
                      new CodeMethodReturnStatement(
                          new CodeArgumentReferenceExpression(ModuleGenBuilder.GetInvoceString(value.Item1))));

                  // add storage key mapping in constructor
                  constructor.Statements.Add(ModuleGenBuilder.AddPropertyValues(ModuleGenBuilder.GetStorageMapString(key.Item1, value.Item1, storage.Prefix, entry.Name, hashers), "_client.StorageKeyDict"));
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
                           (string, List<string>) fullItem = GetFullItemPath(field.TypeId);

                           CodeParameterDeclarationExpression param = new()
                           {
                              Type = new CodeTypeReference(fullItem.Item1),
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

      // TODO (svnscha) I have disabled CreateEvents because it doesn't really work and isn't used yet. This needs to be implemented correctly (check the missing case's).
      //private void CreateEvents(CodeNamespace typeNamespace, CodeConstructor constructor)
      //{
      //   ClassName = Module.Name + "Events";

      //   var events = Module.Events;

      //   if (events != null)
      //   {
      //      if (NodeTypes.TryGetValue(events.TypeId, out NodeType nodeType))
      //      {
      //         var typeDef = nodeType as NodeTypeVariant;

      //         // TODO (svnscha): Why does it crash now with the newest runtime?
      //         if (typeDef.Variants == null)
      //            return;

      //         foreach (var variant in typeDef.Variants)
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
      //               foreach (var field in variant.TypeFields)
      //               {
      //                  var fullItem = GetFullItemPath(field.TypeId);
      //                  codeTypeRef.TypeArguments.Add(new CodeTypeReference(fullItem.Item1));
      //               }
      //            }
      //            eventClass.BaseTypes.Add(codeTypeRef);

      //            // add event key mapping in constructor
      //            // TODO (svnscha) What is with events?
      //            // Console.WriteLine($"case \"{Module.Index}-{variant.Index}\": return typeof({NamespaceName + "." + eventClass.Name});");
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

      //private void CreateConstants(CodeNamespace typeNamespace, CodeConstructor constructor)
      //{
      //   // TODO
      //}

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


               foreach (TypeVariant variant in typeDef.Variants)
               {
                  var enumField = new CodeMemberField(ClassName, variant.Name);

                  // add comment to field if exists
                  enumField.Comments.AddRange(GetComments(variant.Docs, null, variant.Name));

                  targetClass.Members.Add(enumField);
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

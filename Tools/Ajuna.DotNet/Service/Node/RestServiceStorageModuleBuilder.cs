using Ajuna.DotNet.Extensions;
using Ajuna.DotNet.Service.Node.Base;
using Ajuna.NetApi.Model.Meta;
using Ajuna.ServiceLayer.Storage;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;

namespace Ajuna.DotNet.Service.Node
{
   public class RestServiceStorageModuleBuilder : ModuleBuilderBase
   {
      private RestServiceStorageModuleBuilder(string projectName, uint id, PalletModule module, Dictionary<uint, (string, List<string>)> typeDict, Dictionary<uint, NodeType> nodeTypes) :
          base(projectName, id, module, typeDict, nodeTypes)
      {
      }

      public static RestServiceStorageModuleBuilder Init(string projectName, uint id, PalletModule module, Dictionary<uint, (string, List<string>)> typeDict, Dictionary<uint, NodeType> nodeTypes)
      {
         return new RestServiceStorageModuleBuilder(projectName, id, module, typeDict, nodeTypes);
      }

      public override RestServiceStorageModuleBuilder Create()
      {

         if (Module.Storage == null)
         {
            Success = false;
            return this;
         }

         ImportsNamespace.Imports.Add(new CodeNamespaceImport("Ajuna.ServiceLayer.Attributes"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport("Ajuna.ServiceLayer.Storage"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport("System.Threading.Tasks"));

         FileName = Module.Storage.Prefix + "Storage";

         ReferenzName = $"{ProjectName}.Generated.Storage.{FileName}";
         NamespaceName = $"{ProjectName}.Generated.Storage";

         CodeNamespace typeNamespace = new(NamespaceName);
         TargetUnit.Namespaces.Add(typeNamespace);

         CreateStorage(typeNamespace);
         return this;
      }

      private void CreateStorage(CodeNamespace typeNamespace)
      {
         ClassName = Module.Storage.Prefix + "Storage";

         var targetInterface = new CodeTypeDeclaration($"I{ClassName}")
         {
            IsInterface = true

         };
         targetInterface.Comments.AddRange(GetComments(new string[] { $"I{ClassName} interface definition." }));
         targetInterface.BaseTypes.Add(new CodeTypeReference("IStorage"));
         typeNamespace.Types.Add(targetInterface);


         var targetClass = new CodeTypeDeclaration(ClassName)
         {
            IsClass = true,
            TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed
         };

         targetClass.Comments.AddRange(GetComments(new string[] { $"{ClassName} class definition." }));
         targetClass.BaseTypes.Add(new CodeTypeReference(targetInterface.Name));

         typeNamespace.Types.Add(targetClass);

         CodeConstructor constructor = new()
         {
            Attributes = MemberAttributes.Public | MemberAttributes.Final,
         };

         constructor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("IStorageDataProvider"), "storageDataProvider"));
         constructor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("List<IStorageChangeDelegate>"), "storageChangeDelegates"));
         constructor.Comments.AddRange(GetComments(new string[] { $"{ClassName} constructor." }));

         targetClass.Members.Add(constructor);

         CodeMemberMethod initializeAsyncMethod = new()
         {
            Attributes = MemberAttributes.Public | MemberAttributes.Final,
            Name = $"InitializeAsync",
            ReturnType = new CodeTypeReference("async Task")

         };
         var clientParamter = new CodeParameterDeclarationExpression(typeof(IStorageDataProvider), "dataProvider");
         initializeAsyncMethod.Parameters.Add(clientParamter);
         targetClass.Members.Add(initializeAsyncMethod);
         initializeAsyncMethod.Comments.AddRange(GetComments(new string[] { $"Connects to all storages and initializes the change subscription handling." }));

         if (Module.Storage.Entries != null)
         {
            var keyParamter = new CodeParameterDeclarationExpression(typeof(string), "key");
            var dataParamter = new CodeParameterDeclarationExpression(typeof(string), "data");

            foreach (Entry entry in Module.Storage.Entries)
            {
               CodeTypeReference baseReturnType;
               CodeTypeReference returnType;
               CodeExpression[] updateExpression, tryGetExpression;
               if (entry.StorageType == Storage.Type.Plain)
               {
                  (string, List<string>) fullItem = GetFullItemPath(entry.TypeMap.Item1);
                  baseReturnType = new CodeTypeReference(fullItem.Item1);
                  returnType = new CodeTypeReference($"TypedStorage<{fullItem.Item1}>");

                  updateExpression = new CodeExpression[] {
                                            new CodeVariableReferenceExpression(dataParamter.Name)};
                  tryGetExpression = Array.Empty<CodeExpression>();
               }
               else if (entry.StorageType == Storage.Type.Map)
               {
                  TypeMap typeMap = entry.TypeMap.Item2;
                  Storage.Hasher[] hashers = typeMap.Hashers;
                  (string, List<string>) key = GetFullItemPath(typeMap.Key);
                  (string, List<string>) value = GetFullItemPath(typeMap.Value);
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

               field.Comments.AddRange(GetComments(new string[] { $"{field.Name} typed storage field" }));
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


               prop.Comments.AddRange(GetComments(new string[] { $"{field.Name} property" }));
               targetClass.Members.Add(prop);

               // constructor initialize storage properties
               constructor.Statements.Add(new CodeAssignStatement(
                   new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), prop.Name),
               new CodeObjectCreateExpression(field.Type,
                   new CodeExpression[] {
                        new CodePrimitiveExpression($"{Module.Storage.Prefix}.{entry.Name}"),
                        new CodeVariableReferenceExpression("storageDataProvider"),
                        new CodeVariableReferenceExpression("storageChangeDelegates")
                   })));

               // create initialize records foreach storage
               CodeMethodInvokeExpression initializeAsyncInvoke = new(
                   new CodeVariableReferenceExpression($"await {prop.Name}"),
                   "InitializeAsync", new CodeExpression[] {
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
               onUpdateMethod.Comments.AddRange(GetComments(new string[] { $"Implements any storage change for {Module.Storage.Prefix}.{entry.Name}" }));

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

using Substrate.DotNet.Extensions;
using Substrate.DotNet.Service.Node.Base;
using Substrate.NetApi.Model.Meta;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;

namespace Substrate.DotNet.Service.Node
{
   public class RestServiceControllerModuleBuilder : ModuleBuilderBase
   {
      private string NetApiProjectName { get; }

      private RestServiceControllerModuleBuilder(string projectName, string netApiProjectName, uint id, PalletModule module, NodeTypeResolver typeDict, Dictionary<uint, NodeType> nodeTypes) :
          base(projectName, id, module, typeDict, nodeTypes)
      {
         NetApiProjectName = netApiProjectName;
      }

      public static RestServiceControllerModuleBuilder Init(string projectName, string netApiProjectName, uint id, PalletModule module, NodeTypeResolver typeDict, Dictionary<uint, NodeType> nodeTypes)
      {
         return new RestServiceControllerModuleBuilder(projectName, netApiProjectName, id, module, typeDict, nodeTypes);
      }

      public override RestServiceControllerModuleBuilder Create()
      {
         if (Module.Storage == null)
         {
            Success = false;
            return this;
         }

         ImportsNamespace.Imports.Add(new CodeNamespaceImport($"{ProjectName}.Generated.Storage"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport("Microsoft.AspNetCore.Mvc"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport("System.Threading.Tasks"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport("Substrate.ServiceLayer.Attributes"));
         FileName = Module.Storage.Prefix + "Controller";
         ReferenzName = $"{ProjectName}.Generated.Controller.{FileName}";
         NamespaceName = $"{ProjectName}.Generated.Controller";

         CodeNamespace typeNamespace = new(NamespaceName);
         TargetUnit.Namespaces.Add(typeNamespace);

         CreateController(typeNamespace);

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

         string fieldName = $"{Module.Storage.Prefix}Storage";
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
            foreach (Entry entry in Module.Storage.Entries)
            {
               CodeParameterDeclarationExpression parameterDeclaration;
               CodeTypeReference baseReturnType;
               CodeExpression[] codeExpressions;
               if (entry.StorageType == Storage.Type.Plain)
               {
                  NodeTypeResolved fullItem = GetFullItemPath(entry.TypeMap.Item1);
                  baseReturnType = new CodeTypeReference(fullItem.ToString());
                  parameterDeclaration = null;
                  codeExpressions = Array.Empty<CodeExpression>();
               }
               else if (entry.StorageType == Storage.Type.Map)
               {
                  TypeMap typeMap = entry.TypeMap.Item2;
                  Storage.Hasher[] hashers = typeMap.Hashers;
                  NodeTypeResolved value = GetFullItemPath(typeMap.Value);
                  baseReturnType = new CodeTypeReference(value.ToString());
                  parameterDeclaration = new CodeParameterDeclarationExpression(typeof(string), "key");
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

               if (entry.StorageType == Storage.Type.Plain)
               {
                  string prefixName = Module.Name == "System" ? "Frame" : "Pallet";

                  getStorageMethod.CustomAttributes.Add(
                      new CodeAttributeDeclaration("StorageKeyBuilder",
                      new CodeAttributeArgument[] {
                                new CodeAttributeArgument(new CodeTypeOfExpression($"{NetApiProjectName}.Generated.Storage.{Module.Name}Storage")),
                                new CodeAttributeArgument(new CodePrimitiveExpression($"{entry.Name}Params"))
                      }));
               }
               else if (entry.StorageType == Storage.Type.Map)
               {
                  string prefixName = Module.Name == "System" ? "Frame" : "Pallet";

                  getStorageMethod.CustomAttributes.Add(
                      new CodeAttributeDeclaration("StorageKeyBuilder",
                      new CodeAttributeArgument[] {
                                new CodeAttributeArgument(new CodeTypeOfExpression($"{NetApiProjectName}.Generated.Storage.{Module.Name}Storage")),
                                new CodeAttributeArgument(new CodePrimitiveExpression($"{entry.Name}Params")),
                                new CodeAttributeArgument(new CodeTypeOfExpression(GetFullItemPath(entry.TypeMap.Item2.Key).ToString()))
                      }));
               }
               else
               {
                  throw new NotImplementedException();
               }

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
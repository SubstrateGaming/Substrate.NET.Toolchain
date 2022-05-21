using Ajuna.DotNet.Extensions;
using Ajuna.DotNet.Service.Node.Base;
using Ajuna.NetApi.Model.Meta;
using Ajuna.NetApi.Model.Types;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ajuna.DotNet.Service.Node
{
   public class EventModuleBuilder : ModulesBuilderBase
   {
      private EventModuleBuilder(string projectName, uint id, PalletModule[] modules, Dictionary<uint, (string, List<string>)> typeDict, Dictionary<uint, NodeType> nodeTypes) :
          base(projectName, id, modules, typeDict, nodeTypes)
      {
      }

      public static EventModuleBuilder Init(string projectName, uint id, PalletModule[] modules, Dictionary<uint, (string, List<string>)> typeDict, Dictionary<uint, NodeType> nodeTypes)
      {
         return new EventModuleBuilder(projectName, id, modules, typeDict, nodeTypes);
      }

      public override EventModuleBuilder Create()
      {
         #region CREATE

         ImportsNamespace.Imports.Add(new CodeNamespaceImport("System.Threading.Tasks"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport("Ajuna.NetApi.Model.Meta"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport("System.Threading"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport("Ajuna.NetApi.Model.Types"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport("Ajuna.NetApi.Model.Extrinsics"));

         FileName = "NodeEvents";
         NamespaceName = "Ajuna.NetApi.Model.Event";
         ReferenzName = "Ajuna.NetApi.Model.Event";

         CodeNamespace typeNamespace = new(NamespaceName);
         TargetUnit.Namespaces.Add(typeNamespace);

         // add constructor
         CodeConstructor constructor = new()
         {
            Attributes =
             MemberAttributes.Public | MemberAttributes.Final
         };

         foreach (var module in Modules)
         {
            var events = module.Events;

            if (events != null)
            {
               if (NodeTypes.TryGetValue(events.TypeId, out NodeType nodeType))
               {
                  var typeDef = nodeType as NodeTypeVariant;

                  foreach (var variant in typeDef.Variants)
                  {
                     var eventClass = new CodeTypeDeclaration("Event" + variant.Name.MakeMethod())
                     {
                        IsClass = true,
                        TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed
                     };

                     // add comment to variant if exists
                     eventClass.Comments.AddRange(GetComments(variant.Docs, null, variant.Name));

                     var codeTypeRef = new CodeTypeReference("BaseTuple");
                     if (variant.TypeFields != null)
                     {
                        foreach (var field in variant.TypeFields)
                        {
                           var fullItem = GetFullItemPath(field.TypeId);
                           codeTypeRef.TypeArguments.Add(new CodeTypeReference(fullItem.Item1));
                        }
                     }
                     eventClass.BaseTypes.Add(codeTypeRef);

                     // add event key mapping in constructor
                     constructor.Statements.Add(
                         AddPropertyValues(new CodeExpression[] {
                                 new CodeObjectCreateExpression(
                                    new CodeTypeReference(typeof(Tuple<int, int>)),
                                    new CodeExpression[] {
                                        new CodePrimitiveExpression((int) module.Index),
                                        new CodePrimitiveExpression( variant.Index)
                                    }),
                                 new CodeTypeOfExpression(ReferenzName + "." + eventClass.Name)

                         }, "_client.EventKeyDict"));

                     typeNamespace.Types.Add(eventClass);
                  }
               }
            }
         }

         #endregion

         return this;
      }


      private string GetInvoceString(string returnType)
      {
         return "await _client.GetStorageAsync<" + returnType + ">(parameters, token)";
      }

      private CodeMethodInvokeExpression GetStorageString(string module, string item, Storage.Type type, Storage.Hasher[] hashers = null)
      {

         CodeExpression[] codeExpressions =
             new CodeExpression[] {
                        new CodePrimitiveExpression(module),
                        new CodePrimitiveExpression(item),
                        new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(typeof(Storage.Type)), type.ToString())
          };

         // if it is a map fill hashers and key
         if (hashers != null && hashers.Length > 0)
         {

            codeExpressions = new CodeExpression[] {
                        new CodePrimitiveExpression(module),
                        new CodePrimitiveExpression(item),
                        new CodePropertyReferenceExpression(
                            new CodeTypeReferenceExpression(typeof(Storage.Type)), type.ToString()),
                        new CodeArrayCreateExpression(
                            new CodeTypeReference(typeof(Storage.Hasher)),
                                hashers.Select(p => new CodePropertyReferenceExpression(
                                    new CodeTypeReferenceExpression(typeof(Storage.Hasher)), p.ToString())).ToArray()),
                        new CodeArrayCreateExpression(
                            new CodeTypeReference(typeof(IType)),
                            new CodeArgumentReferenceExpression[] {
                                new CodeArgumentReferenceExpression("key") })
                    };
         }

         return new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("RequestGenerator"), "GetStorage", codeExpressions);
      }

      private CodeExpression[] GetStorageMapString(string keyType, string returnType, string module, string item, Storage.Type type, Storage.Hasher[] hashers = null)
      {
         var typeofReturn = new CodeTypeOfExpression(returnType);

         CodeExpression[] result = new CodeExpression[] {
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

      private CodeStatement AddPropertyValues(CodeExpression[] exprs, string variableReference)
      {
         return new CodeExpressionStatement(
             new CodeMethodInvokeExpression(
                 new CodeMethodReferenceExpression(
                     new CodeTypeReferenceExpression(
                         new CodeTypeReference(variableReference)), "Add"), exprs));
      }
   }
}

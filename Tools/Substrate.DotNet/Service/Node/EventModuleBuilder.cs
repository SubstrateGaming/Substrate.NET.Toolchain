using Substrate.DotNet.Extensions;
using Substrate.DotNet.Service.Node.Base;
using Substrate.NetApi.Model.Meta;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;

namespace Substrate.DotNet.Service.Node
{
   public class EventModuleBuilder : ModulesBuilderBase
   {
      private EventModuleBuilder(string projectName, uint id, PalletModule[] modules, NodeTypeResolver typeDict, Dictionary<uint, NodeType> nodeTypes) :
          base(projectName, id, modules, typeDict, nodeTypes)
      {
      }

      public static EventModuleBuilder Init(string projectName, uint id, PalletModule[] modules, NodeTypeResolver typeDict, Dictionary<uint, NodeType> nodeTypes)
      {
         return new EventModuleBuilder(projectName, id, modules, typeDict, nodeTypes);
      }

      public override EventModuleBuilder Create()
      {
         ImportsNamespace.Imports.Add(new CodeNamespaceImport("System.Threading.Tasks"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport("Substrate.NetApi.Model.Meta"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport("System.Threading"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport("Substrate.NetApi.Model.Types"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport("Substrate.NetApi.Model.Extrinsics"));

         FileName = "NodeEvents";
         NamespaceName = "Substrate.NetApi.Model.Event";
         ReferenzName = "Substrate.NetApi.Model.Event";

         CodeNamespace typeNamespace = new(NamespaceName);
         TargetUnit.Namespaces.Add(typeNamespace);

         // add constructor
         CodeConstructor constructor = new()
         {
            Attributes =
             MemberAttributes.Public | MemberAttributes.Final
         };

         foreach (PalletModule module in Modules)
         {
            PalletEvents events = module.Events;

            if (events != null)
            {
               if (NodeTypes.TryGetValue(events.TypeId, out NodeType nodeType))
               {
                  var typeDef = nodeType as NodeTypeVariant;

                  foreach (TypeVariant variant in typeDef.Variants)
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
                        foreach (NodeTypeField field in variant.TypeFields)
                        {
                           NodeTypeResolved fullItem = GetFullItemPath(field.TypeId);
                           codeTypeRef.TypeArguments.Add(new CodeTypeReference(fullItem.ToString()));
                        }
                     }
                     eventClass.BaseTypes.Add(codeTypeRef);

                     // add event key mapping in constructor
                     constructor.Statements.Add(
                         EventModuleBuilder.AddPropertyValues(new CodeExpression[] {
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
         return this;
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
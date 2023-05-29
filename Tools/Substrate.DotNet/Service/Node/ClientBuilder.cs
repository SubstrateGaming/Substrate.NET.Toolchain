using Substrate.DotNet.Service.Node.Base;
using Substrate.NetApi;
using Substrate.NetApi.Model.Extrinsics;
using Substrate.NetApi.Model.Meta;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;

namespace Substrate.DotNet.Service.Node
{
   public class ClientBuilder : ClientBuilderBase
   {
      private ClientBuilder(string projectName, uint id, List<string> moduleNames, NodeTypeResolver typeDict) :
          base(projectName, id, moduleNames, typeDict)
      {
      }

      public static ClientBuilder Init(string projectName, uint id, List<string> moduleNames, NodeTypeResolver typeDict)
      {
         return new ClientBuilder(projectName, id, moduleNames, typeDict);
      }

      public override ClientBuilder Create()
      {
         ClassName = "SubstrateClientExt";
         NamespaceName = $"{ProjectName}.Generated";

         CodeNamespace typeNamespace = new(NamespaceName);
         TargetUnit.Namespaces.Add(typeNamespace);

         ImportsNamespace.Imports.Add(new CodeNamespaceImport("Substrate.NetApi.Model.Meta"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport("Substrate.NetApi.Model.Extrinsics"));

         var targetClass = new CodeTypeDeclaration(ClassName)
         {
            IsClass = true,
            TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed
         };
         targetClass.BaseTypes.Add(new CodeTypeReference(typeof(SubstrateClient)));
         typeNamespace.Types.Add(targetClass);

         CodeConstructor constructor = new()
         {
            Attributes = MemberAttributes.Public | MemberAttributes.Final
         };

         // Add parameters.
         constructor.Parameters.Add(
             new CodeParameterDeclarationExpression(typeof(Uri), "uri"));
         constructor.Parameters.Add(
            new CodeParameterDeclarationExpression(typeof(ChargeType), "chargeType"));

         constructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("uri"));
         constructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("chargeType"));

         targetClass.Members.Add(constructor);

         CodeMemberField storageKeyField = new()
         {
            Attributes = MemberAttributes.Public,
            Name = "StorageKeyDict",
            Type = new CodeTypeReference(typeof(Dictionary<Tuple<string, string>, Tuple<Storage.Hasher[], Type, Type>>)),
         };
         storageKeyField.Comments.AddRange(GetComments(new string[] { $"{storageKeyField.Name} for key definition informations." }, null, null));
         targetClass.Members.Add(storageKeyField);

         constructor.Statements.Add(
             new CodeAssignStatement(
                 new CodeVariableReferenceExpression(storageKeyField.Name),
                 new CodeObjectCreateExpression(storageKeyField.Type, Array.Empty<CodeExpression>())));

         //CodeMemberField eventKeyField = new()
         //{
         //    Attributes = MemberAttributes.Public | MemberAttributes.Static,
         //    Name = "EventKeyDict",
         //    Type = new CodeTypeReference(typeof(Dictionary<Tuple<int, int>, Type>)),
         //};
         //eventKeyField.Comments.AddRange(GetComments(new string[] { $"{eventKeyField.Name} for event definition informations." }, null, null));
         //targetClass.Members.Add(eventKeyField);

         //constructor.Statements.Add(
         //    new CodeAssignStatement(
         //        new CodeVariableReferenceExpression(eventKeyField.Name),
         //        new CodeObjectCreateExpression(eventKeyField.Type, new CodeExpression[] { })));

         foreach (string moduleName in ModuleNames)
         {
            string[] pallets = new string[] { "Storage" }; // , "Call"};

            foreach (string pallet in pallets)
            {
               CodeMemberField clientField = new()
               {
                  Attributes = MemberAttributes.Public,
                  Name = moduleName,
                  Type = new CodeTypeReference(moduleName)
               };
               clientField.Comments.AddRange(GetComments(new string[] { $"{moduleName} storage calls." }, null, null));
               targetClass.Members.Add(clientField);

               CodeFieldReferenceExpression fieldReference =
                   new(new CodeThisReferenceExpression(), moduleName);

               var createPallet = new CodeObjectCreateExpression(moduleName);
               createPallet.Parameters.Add(new CodeThisReferenceExpression());
               constructor.Statements.Add(new CodeAssignStatement(fieldReference, createPallet));
            }
         }

         return this;
      }
   }
}
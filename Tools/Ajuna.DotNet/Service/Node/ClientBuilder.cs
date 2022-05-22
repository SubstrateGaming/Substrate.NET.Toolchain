using Ajuna.DotNet.Service.Node.Base;
using Ajuna.NetApi;
using Ajuna.NetApi.Model.Meta;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ajuna.DotNet.Service.Node
{
   public class ClientBuilder : ClientBuilderBase
   {
      private ClientBuilder(string projectName, uint id, List<(string, List<string>)> moduleNames, Dictionary<uint, (string, List<string>)> typeDict) :
          base(projectName, id, moduleNames, typeDict)
      {
      }

      public static ClientBuilder Init(string projectName, uint id, List<(string, List<string>)> moduleNames, Dictionary<uint, (string, List<string>)> typeDict)
      {
         return new ClientBuilder(projectName, id, moduleNames, typeDict);
      }

      public override ClientBuilder Create()
      {
         ClassName = "SubstrateClientExt";
         NamespaceName = $"{ProjectName}.Generated";

         CodeNamespace typeNamespace = new(NamespaceName);
         TargetUnit.Namespaces.Add(typeNamespace);

         ImportsNamespace.Imports.Add(new CodeNamespaceImport("Ajuna.NetApi.Model.Meta"));

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
         constructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("uri"));
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

         foreach ((string, List<string>) tuple in ModuleNames)
         {
            string[] pallets = new string[] { "Storage" }; // , "Call"};

            foreach (string pallet in pallets)
            {
               string name = tuple.Item1.Split('.').Last() + pallet;
               string referenceName = tuple.Item2[0] + "." + name;

               CodeMemberField clientField = new()
               {
                  Attributes = MemberAttributes.Public,
                  Name = name,
                  Type = new CodeTypeReference(referenceName)
               };
               clientField.Comments.AddRange(GetComments(new string[] { $"{name} storage calls." }, null, null));
               targetClass.Members.Add(clientField);

               CodeFieldReferenceExpression fieldReference =
                   new(new CodeThisReferenceExpression(), name);

               var createPallet = new CodeObjectCreateExpression(referenceName);
               createPallet.Parameters.Add(new CodeThisReferenceExpression());
               constructor.Statements.Add(new CodeAssignStatement(fieldReference, createPallet));
            }
         }

         return this;
      }

   }

}

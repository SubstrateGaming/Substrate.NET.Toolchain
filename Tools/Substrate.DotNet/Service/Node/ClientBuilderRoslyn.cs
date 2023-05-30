using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Substrate.DotNet.Service.Node.Base;
using System.Collections.Generic;

namespace Substrate.DotNet.Service.Node
{
   public class ClientBuilderRoslyn : ClientBuilderBaseRoslyn
   {
      private ClientBuilderRoslyn(string projectName, uint id, List<string> moduleNames, NodeTypeResolver typeDict) :
          base(projectName, id, moduleNames, typeDict)
      {
      }

      public static ClientBuilderRoslyn Init(string projectName, uint id, List<string> moduleNames, NodeTypeResolver typeDict)
      {
         return new ClientBuilderRoslyn(projectName, id, moduleNames, typeDict);
      }

      public override ClientBuilderRoslyn Create()
      {
         ClassName = "SubstrateClientExt";
         NamespaceName = $"{ProjectName}.Generated";

         NamespaceDeclarationSyntax namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(NamespaceName));

         // Using Directives
         namespaceDeclaration = namespaceDeclaration.AddUsings(
             SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Substrate.NetApi.Model.Meta")),
             SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Substrate.NetApi.Model.Extrinsics"))
         );

         ClassDeclarationSyntax targetClass = SyntaxFactory.ClassDeclaration(ClassName)
             .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.SealedKeyword))
             .WithBaseList(
                 SyntaxFactory.BaseList().AddTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("Substrate.NetApi.SubstrateClient")))
             );

         // Constructor
         ConstructorDeclarationSyntax constructor = SyntaxFactory.ConstructorDeclaration(ClassName)
             .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
             .AddParameterListParameters(
                 SyntaxFactory.Parameter(SyntaxFactory.Identifier("uri")).WithType(SyntaxFactory.ParseTypeName("System.Uri")),
                 SyntaxFactory.Parameter(SyntaxFactory.Identifier("chargeType")).WithType(SyntaxFactory.ParseTypeName("ChargeType"))
             )
             .WithInitializer(
                 SyntaxFactory.ConstructorInitializer(SyntaxKind.BaseConstructorInitializer)
                 .AddArgumentListArguments(
                     SyntaxFactory.Argument(SyntaxFactory.IdentifierName("uri")),
                     SyntaxFactory.Argument(SyntaxFactory.IdentifierName("chargeType"))
                 )
             );

         //// Field declaration
         //FieldDeclarationSyntax storageKeyField = SyntaxFactory.FieldDeclaration(
         //        SyntaxFactory.VariableDeclaration(
         //            SyntaxFactory.ParseTypeName("Dictionary<System.Tuple<string, string>, System.Tuple<Substrate.NetApi.Model.Meta.Storage.Hasher[], System.Type, System.Type>>"))
         //        .AddVariables(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("StorageKeyDict"))))
         //    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
         //    .WithLeadingTrivia(GetCommentsRoslyn(new string[] { $"StorageKeyDict for key definition informations." }, null));
         //targetClass = targetClass.AddMembers(storageKeyField);

         // Initialize field in constructor
         //constructor = constructor.WithBody(SyntaxFactory.Block(
         //    SyntaxFactory.ExpressionStatement(
         //        SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
         //            SyntaxFactory.IdentifierName("StorageKeyDict"),
         //            SyntaxFactory.ObjectCreationExpression(
         //                SyntaxFactory.ParseTypeName("Dictionary<System.Tuple<string, string>, System.Tuple<Substrate.NetApi.Model.Meta.Storage.Hasher[], System.Type, System.Type>>"))
         //            .AddArgumentListArguments()))));

         // Module related logic
         foreach (string moduleName in ModuleNames)
         {
            string[] pallets = new string[] { "Storage" }; // , "Call"};

            foreach (string pallet in pallets)
            {

               // Property declaration
               PropertyDeclarationSyntax moduleProperty = SyntaxFactory.PropertyDeclaration(
                       SyntaxFactory.ParseTypeName(moduleName),
                       SyntaxFactory.Identifier(moduleName))
                   .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                   .AddAccessorListAccessors(
                       SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                   )
                   .WithLeadingTrivia(GetCommentsRoslyn(new string[] { $"{moduleName} storage calls." }, null));


               // Adding Field to Class
               targetClass = targetClass.AddMembers(moduleProperty);

               // Initialize field in constructor
               ExpressionStatementSyntax fieldAssignment = SyntaxFactory.ExpressionStatement(
                   SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                       SyntaxFactory.IdentifierName(moduleName),
                       SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName(moduleName))
                           .AddArgumentListArguments(SyntaxFactory.Argument(SyntaxFactory.ThisExpression()))));

               // Adding statement to constructor's body
               constructor = constructor.AddBodyStatements(fieldAssignment);
            }
         }

         // Reassigning the updated constructor to the class
         targetClass = targetClass.RemoveNode(constructor, SyntaxRemoveOptions.KeepNoTrivia);
         targetClass = targetClass.AddMembers(constructor);

         // Adding Class to Namespace
         namespaceDeclaration = namespaceDeclaration.RemoveNode(targetClass, SyntaxRemoveOptions.KeepNoTrivia);
         namespaceDeclaration = namespaceDeclaration.AddMembers(targetClass);

         // Adding Namespace to Compilation Unit
         TargetUnit = TargetUnit.AddMembers(namespaceDeclaration);

         return this;
      }
   }
}
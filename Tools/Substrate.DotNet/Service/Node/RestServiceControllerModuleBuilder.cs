using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Substrate.DotNet.Extensions;
using Substrate.DotNet.Service.Node.Base;
using Substrate.NetApi.Model.Meta;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

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

         FileName = Module.Storage.Prefix + "Controller";
         ReferenzName = $"{ProjectName}.Generated.Controller.{FileName}";
         NamespaceName = $"{ProjectName}.Generated.Controller";

         SyntaxList<UsingDirectiveSyntax> usingDirectives = new SyntaxList<UsingDirectiveSyntax>()
             .Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{ProjectName}.Generated.Storage")))
             .Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.AspNetCore.Mvc")))
             .Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Threading.Tasks")))
             .Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Substrate.ServiceLayer.Attributes")));

         NamespaceDeclarationSyntax namespaceDeclaration = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName(NamespaceName));

         CreateController(namespaceDeclaration);

         TargetUnit = TargetUnit.AddUsings(usingDirectives.ToArray());

         return this;
      }

      private void CreateController(NamespaceDeclarationSyntax namespaceDeclaration)
      {
         ClassName = FileName;

         // Create ControllerBase attribute
         AttributeListSyntax baseAttribute = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                 SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("ApiController()"))));

         // Create Route attribute
         AttributeListSyntax routeAttribute = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                 SyntaxFactory.Attribute(
                     SyntaxFactory.IdentifierName("Route"),
                     SyntaxFactory.AttributeArgumentList(SyntaxFactory.SingletonSeparatedList(
                         SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("[controller]"))))))));

         // Creating class declaration
         ClassDeclarationSyntax targetClass = SyntaxFactory.ClassDeclaration(ClassName)
             .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.SealedKeyword))
             .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.IdentifierName("ControllerBase")))
             .AddAttributeLists(baseAttribute, routeAttribute)
             .WithLeadingTrivia(GetCommentsRoslyn(new string[] { $"{ClassName} controller to access storages." }));

         // Assuming that fieldName and Module.Storage.Prefix are string variables
         string fieldName = $"{Module.Storage.Prefix}Storage";
         string fieldNamePublic = char.ToLower(fieldName[0]) + fieldName.Substring(1);
         string fieldNamePrivate = "_" + fieldNamePublic;

         // Field declaration
         FieldDeclarationSyntax field = SyntaxFactory.FieldDeclaration(
             SyntaxFactory.VariableDeclaration(
                 SyntaxFactory.IdentifierName($"I{fieldName}"),
                 SyntaxFactory.SingletonSeparatedList(
                     SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(fieldNamePrivate)))))
             .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));


         targetClass = targetClass.AddMembers(field);

         // Create constructor
         ConstructorDeclarationSyntax constructor = SyntaxFactory
            .ConstructorDeclaration(SyntaxFactory.Identifier(ClassName))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .WithLeadingTrivia(GetCommentsRoslyn(new string[] { $"{ClassName} constructor." }))
            .WithParameterList(
               SyntaxFactory.ParameterList(
                  SyntaxFactory.SeparatedList(new[] {
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(fieldNamePublic))
                    .WithType(SyntaxFactory.ParseTypeName($"I{fieldName}"))
                  })))
             .WithBody(
                 SyntaxFactory.Block(
                     SyntaxFactory.ExpressionStatement(
                         SyntaxFactory.AssignmentExpression(
                             SyntaxKind.SimpleAssignmentExpression,
                             SyntaxFactory.IdentifierName(fieldNamePrivate),
                             SyntaxFactory.IdentifierName(fieldNamePublic)))));
         // Add constructor to class
         targetClass = targetClass.AddMembers(constructor);


         // Assuming you have a method that accepts a string and returns a MethodDeclarationSyntax object
         // Iterate over the storage entries and create the methods
         if (Module.Storage.Entries != null)
         {
            foreach (Entry entry in Module.Storage.Entries)
            {
               // Assuming CreateMethod is a method that creates a MethodDeclarationSyntax object for a given entry
               MethodDeclarationSyntax methodDeclaration = CreateMethod(fieldNamePrivate, entry);
               methodDeclaration = methodDeclaration
                  .WithLeadingTrivia(GetCommentsRoslyn(entry.Docs, null, entry.Name));

               targetClass = targetClass.AddMembers(methodDeclaration);
            }
         }

         namespaceDeclaration = namespaceDeclaration.AddMembers(targetClass);

         TargetUnit = TargetUnit.AddMembers(namespaceDeclaration);
      }

      private MethodDeclarationSyntax CreateMethod(string fieldNamePrivate, Entry entry)
      {
         // Prepare the method parameters and return type based on the entry.StorageType
         TypeSyntax baseReturnType;
         var methodParameters = new List<ParameterSyntax>();

         ExpressionSyntax invokeExpression;

         if (entry.StorageType == Storage.Type.Plain)
         {
            NodeTypeResolved fullItem = GetFullItemPath(entry.TypeMap.Item1);
            baseReturnType = SyntaxFactory.ParseTypeName(fullItem.ToString());
            invokeExpression = SyntaxFactory.IdentifierName("");
         }
         else if (entry.StorageType == Storage.Type.Map)
         {
            TypeMap typeMap = entry.TypeMap.Item2;
            Storage.Hasher[] hashers = typeMap.Hashers;
            NodeTypeResolved value = GetFullItemPath(typeMap.Value);
            baseReturnType = SyntaxFactory.ParseTypeName(value.ToString());

            methodParameters.Add(SyntaxFactory.Parameter(SyntaxFactory.Identifier("key"))
                .WithType(SyntaxFactory.ParseTypeName("string")));

            invokeExpression = SyntaxFactory.IdentifierName("key");
         }
         else
         {
            throw new NotImplementedException();
         }

         string indentifier = $"Get{entry.Name}";

         MethodDeclarationSyntax getStorageMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("IActionResult"), indentifier)
             .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
             .WithBody(SyntaxFactory.Block(
                 SyntaxFactory.ReturnStatement(
                     SyntaxFactory.InvocationExpression(
                         SyntaxFactory.IdentifierName("Ok"),
                         SyntaxFactory.ArgumentList(
                             SyntaxFactory.SingletonSeparatedList(
                                 SyntaxFactory.Argument(
                                     SyntaxFactory.InvocationExpression(
                                         SyntaxFactory.MemberAccessExpression(
                                             SyntaxKind.SimpleMemberAccessExpression,
                                             SyntaxFactory.IdentifierName(fieldNamePrivate),
                                             SyntaxFactory.IdentifierName(indentifier)),
                                         SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(invokeExpression)))))))))));



         // Add parameters to method
         foreach (ParameterSyntax parameter in methodParameters)
         {
            getStorageMethod = getStorageMethod.AddParameterListParameters(parameter);
         }

         // Add HttpGet attribute
         AttributeListSyntax getAttribute = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                 SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("HttpGet"),
                 SyntaxFactory.AttributeArgumentList(SyntaxFactory.SingletonSeparatedList(
                     SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(entry.Name))))))));
         getStorageMethod = getStorageMethod.AddAttributeLists(getAttribute);

         // Add ProducesResponseType attribute
         AttributeListSyntax responseAttribute = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                 SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("ProducesResponseType"),
                 SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(new[] {
                SyntaxFactory.AttributeArgument(SyntaxFactory.TypeOfExpression(baseReturnType)),
                SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(200))) })))));
         getStorageMethod = getStorageMethod.AddAttributeLists(responseAttribute);

         // Add StorageKeyBuilder attribute
         AttributeListSyntax storageAttribute;

         if (entry.StorageType == Storage.Type.Plain)
         {
            string prefixName = Module.Name == "System" ? "Frame" : "Pallet";

            storageAttribute = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("StorageKeyBuilder"),
                    SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(new[] {
                    SyntaxFactory.AttributeArgument(SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName($"{NetApiProjectName}.Generated.Storage.{Module.Name}Storage"))),
                    SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal($"{entry.Name}Params"))) })))));
         }
         else if (entry.StorageType == Storage.Type.Map)
         {
            string prefixName = Module.Name == "System" ? "Frame" : "Pallet";

            storageAttribute = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("StorageKeyBuilder"),
                    SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(new[] {
                    SyntaxFactory.AttributeArgument(SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName($"{NetApiProjectName}.Generated.Storage.{Module.Name}Storage"))),
                    SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal($"{entry.Name}Params"))),
                    SyntaxFactory.AttributeArgument(SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName(GetFullItemPath(entry.TypeMap.Item2.Key).ToString()))) })))));
         }
         else
         {
            throw new NotImplementedException();
         }

         getStorageMethod = getStorageMethod.AddAttributeLists(storageAttribute);

         return getStorageMethod;
      }

   }
}
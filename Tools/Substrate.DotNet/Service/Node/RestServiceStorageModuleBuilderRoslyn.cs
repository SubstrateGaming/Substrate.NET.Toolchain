using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Substrate.DotNet.Extensions;
using Substrate.DotNet.Service.Node.Base;
using Substrate.NetApi.Model.Meta;
using Substrate.ServiceLayer.Storage;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Data;

namespace Substrate.DotNet.Service.Node
{
   public class RestServiceStorageModuleBuilderRoslyn : ModuleBuilderBaseRoslyn
   {
      private RestServiceStorageModuleBuilderRoslyn(string projectName, uint id, PalletModule module, NodeTypeResolver typeDict, Dictionary<uint, NodeType> nodeTypes) :
          base(projectName, id, module, typeDict, nodeTypes)
      {
      }

      public static RestServiceStorageModuleBuilderRoslyn Init(string projectName, uint id, PalletModule module, NodeTypeResolver typeDict, Dictionary<uint, NodeType> nodeTypes)
      {
         return new RestServiceStorageModuleBuilderRoslyn(projectName, id, module, typeDict, nodeTypes);
      }

      public override RestServiceStorageModuleBuilderRoslyn Create()
      {
         if (Module.Storage == null)
         {
            Success = false;
            return this;
         }

         FileName = Module.Storage.Prefix + "Storage";
         ReferenzName = $"{ProjectName}.Generated.Storage.{FileName}";
         NamespaceName = $"{ProjectName}.Generated.Storage";

         SyntaxList<UsingDirectiveSyntax> usingDirectives = new SyntaxList<UsingDirectiveSyntax>()
          .Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Substrate.ServiceLayer.Attributes")))
          .Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Substrate.ServiceLayer.Storage")))
          .Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Threading.Tasks")));

         NamespaceDeclarationSyntax namespaceDeclaration = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName(NamespaceName));

         TargetUnit = TargetUnit.AddUsings(usingDirectives.ToArray());
         TargetUnit = TargetUnit.AddMembers(CreateStorage(namespaceDeclaration));

         return this;
      }

      private NamespaceDeclarationSyntax CreateStorage(NamespaceDeclarationSyntax namespaceDeclaration)
      {
         // Setting ClassName
         ClassName = Module.Storage.Prefix + "Storage";

         // Creating the interface declaration
         InterfaceDeclarationSyntax targetInterface = SyntaxFactory
            .InterfaceDeclaration($"I{ClassName}")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("IStorage")))
            .WithLeadingTrivia(GetCommentsRoslyn(new string[] { $"I{ClassName} interface definition" }));

         // Creating the class declaration
         ClassDeclarationSyntax targetClass = SyntaxFactory.ClassDeclaration(ClassName)
             .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.SealedKeyword))
             .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(targetInterface.Identifier.Text)))
             .WithLeadingTrivia(GetCommentsRoslyn(new string[] { $"{ClassName} class definition" }));

         // Creating the constructor
         ConstructorDeclarationSyntax constructor = SyntaxFactory.ConstructorDeclaration(ClassName)
             .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
             .WithParameterList(SyntaxFactory.ParameterList()
                 .AddParameters(
                     SyntaxFactory.Parameter(SyntaxFactory.Identifier("storageDataProvider"))
                         .WithType(SyntaxFactory.ParseTypeName("IStorageDataProvider")),
                     SyntaxFactory.Parameter(SyntaxFactory.Identifier("storageChangeDelegates"))
                         .WithType(SyntaxFactory.ParseTypeName("List<IStorageChangeDelegate>"))
                 ))
             .WithLeadingTrivia(GetCommentsRoslyn(new string[] { $"{ClassName} constructor" }));

         // Creating the InitializeAsync method
         MethodDeclarationSyntax initializeAsyncMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("Task"), "InitializeAsync")
             .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
             .AddParameterListParameters(
                 SyntaxFactory.Parameter(SyntaxFactory.Identifier("dataProvider"))
                     .WithType(SyntaxFactory.ParseTypeName("Substrate.ServiceLayer.Storage.IStorageDataProvider"))
             )
             .WithLeadingTrivia(GetCommentsRoslyn(new string[] { "Connects to all storages and initializes the change subscription handling" }));

         if (Module.Storage.Entries != null)
         {
            ParameterSyntax keyParamter = SyntaxFactory.Parameter(SyntaxFactory.Identifier("key"))
                .WithType(SyntaxFactory.ParseTypeName("string"));

            ParameterSyntax dataParamter = SyntaxFactory.Parameter(SyntaxFactory.Identifier("data"))
                .WithType(SyntaxFactory.ParseTypeName("string"));

            var constructorStatements = new List<StatementSyntax>();
            var initializeStatements = new List<StatementSyntax>();

            //var fields = new List<FieldDeclarationSyntax>();
            var props = new List<PropertyDeclarationSyntax>();

            var interfaceMethods = new List<MethodDeclarationSyntax>();
            var methodOnAndGet = new List<MethodDeclarationSyntax>(); 

            foreach (Entry entry in Module.Storage.Entries)
            {
               TypeSyntax baseReturnType;
               TypeSyntax returnType;
               ArgumentListSyntax updateExpression, tryGetExpression;

               if (entry.StorageType == Storage.Type.Plain)
               {
                  NodeTypeResolved fullItem = GetFullItemPath(entry.TypeMap.Item1);
                  baseReturnType = SyntaxFactory.ParseTypeName(fullItem.ToString());
                  returnType = SyntaxFactory.ParseTypeName($"TypedStorage<{fullItem}>");

                  updateExpression = SyntaxFactory.ArgumentList().AddArguments(
                      SyntaxFactory.Argument(SyntaxFactory.IdentifierName(dataParamter.Identifier)));

                  tryGetExpression = SyntaxFactory.ArgumentList();
               }
               else if (entry.StorageType == Storage.Type.Map)
               {
                  TypeMap typeMap = entry.TypeMap.Item2;
                  Storage.Hasher[] hashers = typeMap.Hashers;
                  NodeTypeResolved key = GetFullItemPath(typeMap.Key);
                  NodeTypeResolved value = GetFullItemPath(typeMap.Value);
                  baseReturnType = SyntaxFactory.ParseTypeName(value.ToString());
                  returnType = SyntaxFactory.ParseTypeName($"TypedMapStorage<{value}>");

                  updateExpression = SyntaxFactory.ArgumentList().AddArguments(
                      SyntaxFactory.Argument(SyntaxFactory.IdentifierName(keyParamter.Identifier)),
                      SyntaxFactory.Argument(SyntaxFactory.IdentifierName(dataParamter.Identifier)));

                  tryGetExpression = SyntaxFactory.ArgumentList().AddArguments(
                      SyntaxFactory.Argument(SyntaxFactory.IdentifierName(keyParamter.Identifier)),
                      SyntaxFactory.Argument(
                          SyntaxFactory.DeclarationExpression(
                              baseReturnType,
                              SyntaxFactory.SingleVariableDesignation(SyntaxFactory.Identifier("result"))))
                      .WithRefOrOutKeyword(SyntaxFactory.Token(SyntaxKind.OutKeyword))
                  );

               }
               else
               {
                  throw new NotImplementedException();
               }

               string fieldName = $"{entry.Name.MakePrivateField()}TypedStorage";

               // create typed storage field
               //FieldDeclarationSyntax field = SyntaxFactory.FieldDeclaration(
               //    SyntaxFactory.VariableDeclaration(returnType)
               //        .AddVariables(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(fieldName))))
               //    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
               //   .WithLeadingTrivia(GetCommentsRoslyn(new string[] { $"{entry.Name} typed storage field" }));
               //fields.Add(field);

               // create typed storage property
               string propName = $"{entry.Name}TypedStorage";
               PropertyDeclarationSyntax prop = SyntaxFactory.PropertyDeclaration(returnType, SyntaxFactory.Identifier(propName))
                   .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                   .AddAccessorListAccessors(
                       SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                           .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                       SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                           .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)))
                   .WithLeadingTrivia(GetCommentsRoslyn(new string[] { $"{propName} property" }));
               props.Add(prop);

               // constructor initialize storage properties
               constructorStatements.Add(SyntaxFactory.ExpressionStatement(
                           SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                               SyntaxFactory.IdentifierName(prop.Identifier.Text),
                               SyntaxFactory.ObjectCreationExpression(prop.Type)
                                   .WithArgumentList(SyntaxFactory.ArgumentList()
                                       .AddArguments(
                                           SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal($"{Module.Storage.Prefix}.{entry.Name}"))),
                                           SyntaxFactory.Argument(SyntaxFactory.IdentifierName("storageDataProvider")),
                                           SyntaxFactory.Argument(SyntaxFactory.IdentifierName("storageChangeDelegates")))))));

               // create initialize records foreach storage
               initializeStatements.Add(SyntaxFactory.ExpressionStatement(
                       SyntaxFactory.InvocationExpression(
                           SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                               SyntaxFactory.IdentifierName($"await {prop.Identifier}"),
                               SyntaxFactory.IdentifierName("InitializeAsync")))
                           .WithArgumentList(SyntaxFactory.ArgumentList()
                               .AddArguments(
                                   SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(Module.Storage.Prefix))),
                                   SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(entry.Name)))))));
               
               // create on update
               MethodDeclarationSyntax onUpdateMethod = SyntaxFactory.MethodDeclaration(
                       SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                       SyntaxFactory.Identifier($"OnUpdate{entry.Name}"))
                   .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                   .AddAttributeLists(
                       SyntaxFactory.AttributeList(
                           SyntaxFactory.SeparatedList(
                               new AttributeSyntax[] {
                    SyntaxFactory.Attribute(
                        SyntaxFactory.IdentifierName("StorageChange"),
                        SyntaxFactory.AttributeArgumentList(
                            SyntaxFactory.SeparatedList(
                                new AttributeArgumentSyntax[]{
                                    SyntaxFactory.AttributeArgument(
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.StringLiteralExpression,
                                            SyntaxFactory.Literal(Module.Storage.Prefix))),
                                    SyntaxFactory.AttributeArgument(
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.StringLiteralExpression,
                                            SyntaxFactory.Literal(entry.Name)))})))})))
                   .WithBody(
                       SyntaxFactory.Block(
                           SyntaxFactory.ExpressionStatement(
                               SyntaxFactory.InvocationExpression(
                                   SyntaxFactory.MemberAccessExpression(
                                       SyntaxKind.SimpleMemberAccessExpression,
                                       SyntaxFactory.IdentifierName(prop.Identifier.Text),
                                       SyntaxFactory.IdentifierName("Update")
                                   ),
                                   SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(
                                       updateExpression.Arguments.Select(expr => expr)))))));
               onUpdateMethod = onUpdateMethod.WithLeadingTrivia(GetCommentsRoslyn(new string[] { $"Implements any storage change for {Module.Storage.Prefix}.{entry.Name}" }));


               // create get and gets
               MethodDeclarationSyntax getInterfaceMethod = SyntaxFactory
                  .MethodDeclaration(baseReturnType, $"Get{entry.Name}")
                   .WithLeadingTrivia(GetCommentsRoslyn(entry.Docs, null, entry.Name))
                   .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

               MethodDeclarationSyntax getStorageMethod = SyntaxFactory
                  .MethodDeclaration(baseReturnType, $"Get{entry.Name}")
                  .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                  .WithLeadingTrivia(GetCommentsRoslyn(entry.Docs, null, entry.Name));

               if (tryGetExpression.Arguments.Count == 0)
               {
                  onUpdateMethod = onUpdateMethod
                     .AddParameterListParameters(dataParamter);

                  getStorageMethod = getStorageMethod.WithBody(
                      SyntaxFactory.Block(
                          SyntaxFactory.ReturnStatement(
                              SyntaxFactory.InvocationExpression(
                                  SyntaxFactory.MemberAccessExpression(
                                      SyntaxKind.SimpleMemberAccessExpression,
                                      SyntaxFactory.IdentifierName(prop.Identifier.Text),
                                      SyntaxFactory.IdentifierName("Get"))))));
               }
               else
               {
                  onUpdateMethod = onUpdateMethod
                     .AddParameterListParameters(keyParamter, dataParamter);

                  getInterfaceMethod = getInterfaceMethod.AddParameterListParameters(
                      SyntaxFactory.Parameter(keyParamter.Identifier)
                          .WithType(keyParamter.Type));

                  getStorageMethod = getStorageMethod.AddParameterListParameters(
                   SyntaxFactory.Parameter(keyParamter.Identifier)
                       .WithType(keyParamter.Type));

                  getStorageMethod = getStorageMethod.WithBody(
                   SyntaxFactory.Block(
                       SyntaxFactory.IfStatement(
                           SyntaxFactory.BinaryExpression(
                               SyntaxKind.EqualsExpression,
                               SyntaxFactory.IdentifierName("key"),
                               SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)),
                           SyntaxFactory.Block(
                               SyntaxFactory.ReturnStatement(
                                   SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)))),
                       SyntaxFactory.IfStatement(
                           SyntaxFactory.InvocationExpression(
                               SyntaxFactory.MemberAccessExpression(
                                   SyntaxKind.SimpleMemberAccessExpression,
                                   SyntaxFactory.IdentifierName(prop.Identifier.Text),
                                   SyntaxFactory.IdentifierName("Dictionary.TryGetValue")),
                               tryGetExpression),
                           SyntaxFactory.Block(
                               SyntaxFactory.ReturnStatement(
                                   SyntaxFactory.IdentifierName("result"))),
                           SyntaxFactory.ElseClause(
                               SyntaxFactory.ReturnStatement(
                                   SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression))))));

               }

               interfaceMethods.Add(getInterfaceMethod);

               methodOnAndGet.Add(onUpdateMethod);
               methodOnAndGet.Add(getStorageMethod);
            }

            //targetClass = targetClass.AddMembers(fields.ToArray());
            targetInterface = targetInterface.AddMembers(interfaceMethods.ToArray());

            constructor = constructor.WithBody(SyntaxFactory.Block(constructorStatements));
            targetClass = targetClass.AddMembers(constructor);

            targetClass = targetClass.AddMembers(props.ToArray());

            initializeAsyncMethod = initializeAsyncMethod.WithBody(SyntaxFactory.Block(initializeStatements));
            targetClass = targetClass.AddMembers(initializeAsyncMethod);

            targetClass = targetClass.AddMembers(methodOnAndGet.ToArray());

            namespaceDeclaration = namespaceDeclaration.AddMembers(targetInterface);

            namespaceDeclaration = namespaceDeclaration.AddMembers(targetClass);

         }

         return namespaceDeclaration;
      }

   }
}
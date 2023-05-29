using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Substrate.DotNet.Extensions;
using Substrate.DotNet.Service.Node.Base;
using Substrate.NetApi.Model.Extrinsics;
using Substrate.NetApi.Model.Meta;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Substrate.DotNet.Service.Node
{
   public class ModuleGenBuilderRoslyn : ModuleBuilderBaseRoslyn
   {
      private ModuleGenBuilderRoslyn(string projectName, uint id, PalletModule module, NodeTypeResolver typeDict, Dictionary<uint, NodeType> nodeTypes) :
          base(projectName, id, module, typeDict, nodeTypes)
      {
      }

      public static ModuleGenBuilderRoslyn Init(string projectName, uint id, PalletModule module, NodeTypeResolver typeDict, Dictionary<uint, NodeType> nodeTypes)
      {
         return new ModuleGenBuilderRoslyn(projectName, id, module, typeDict, nodeTypes);
      }

      public override ModuleGenBuilderRoslyn Create()
      {
         UsingDirectiveSyntax[] usings = new[]
         {
            "System.Threading.Tasks",
            "Substrate.NetApi.Model.Meta",
            "System.Threading",
            "Substrate.NetApi",
            "Substrate.NetApi.Model.Types",
            "Substrate.NetApi.Model.Extrinsics",
         }.Select(u => SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(u))).ToArray();

         FileName = "Main" + Module.Name;
         NamespaceName = $"{ProjectName}.Generated.Storage";
         ReferenzName = NamespaceName;

         NamespaceDeclarationSyntax typeNamespace = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName(NamespaceName));

         typeNamespace = CreateStorage(typeNamespace);
         typeNamespace = CreateCalls(typeNamespace);
         typeNamespace = CreateEvents(typeNamespace);
         typeNamespace = CreateConstants(typeNamespace);
         typeNamespace = CreateErrors(typeNamespace);

         TargetUnit = TargetUnit
            .AddUsings(usings)
            .AddMembers(typeNamespace);

         return this;
      }

      private NamespaceDeclarationSyntax CreateStorage(NamespaceDeclarationSyntax typeNamespace)
      {
         ClassName = Module.Name + "Storage";

         PalletStorage storage = Module.Storage;

         ConstructorDeclarationSyntax constructor = SyntaxFactory
            .ConstructorDeclaration(ClassName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .WithBody(SyntaxFactory.Block());

         // Create the class
         ClassDeclarationSyntax targetClass = SyntaxFactory.ClassDeclaration(ClassName)
              .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.SealedKeyword)));

         // Create the client field.
         FieldDeclarationSyntax clientField = SyntaxFactory.FieldDeclaration(
                 SyntaxFactory.VariableDeclaration(
                     SyntaxFactory.ParseTypeName("SubstrateClientExt"))
                 .WithVariables(
                     SyntaxFactory.SingletonSeparatedList(
                         SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("_client")))))
             .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)))
             .WithLeadingTrivia(SyntaxFactory.Comment("// Substrate client for the storage calls."));
         targetClass = targetClass.AddMembers(clientField);

         // Add parameters.
         constructor = constructor.AddParameterListParameters(
             SyntaxFactory.Parameter(SyntaxFactory.Identifier("client"))
                 .WithType(SyntaxFactory.ParseTypeName(clientField.Declaration.Type.ToString())));

         // Assignment statement for the constructor.
         constructor = constructor.AddBodyStatements(
             SyntaxFactory.ExpressionStatement(
                 SyntaxFactory.AssignmentExpression(
                     SyntaxKind.SimpleAssignmentExpression,
                     SyntaxFactory.IdentifierName("_client"),
                     SyntaxFactory.IdentifierName("client"))));

         if (storage?.Entries != null)
         {
            foreach (Entry entry in storage.Entries)
            {
               string storageParams = entry.Name + "Params";

               // Create static method
               MethodDeclarationSyntax parameterMethod = SyntaxFactory
                  .MethodDeclaration(SyntaxFactory.ParseTypeName("string"), storageParams)
                  .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                  .WithLeadingTrivia(GetCommentsRoslyn(entry.Docs, null, storageParams)); // Assuming GetComments() returns a string

               MethodDeclarationSyntax storageMethod;
               ExpressionStatementSyntax methodInvoke;
               NodeTypeResolved returnValueStr;

               if (entry.StorageType == Storage.Type.Plain)
               {
                  returnValueStr = GetFullItemPath(entry.TypeMap.Item1);

                  storageMethod = SyntaxFactory
                     .MethodDeclaration(SyntaxFactory.ParseTypeName($"Task<{returnValueStr}>"), entry.Name);

                  parameterMethod = parameterMethod.AddBodyStatements(
                     SyntaxFactory.ReturnStatement(GetStorageStringRoslyn(storage.Prefix, entry.Name, entry.StorageType)));

                  methodInvoke = SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName(parameterMethod.Identifier)));

                  // add storage key mapping in constructor
                  constructor = constructor.AddBodyStatements(AddPropertyValuesRoslyn(GetStorageMapStringRoslyn("", returnValueStr.ToString(), storage.Prefix, entry.Name), "_client.StorageKeyDict"));
               }
               else if (entry.StorageType == Storage.Type.Map)
               {
                  TypeMap typeMap = entry.TypeMap.Item2;
                  Storage.Hasher[] hashers = typeMap.Hashers;
                  NodeTypeResolved key = GetFullItemPath(typeMap.Key);
                  returnValueStr = GetFullItemPath(typeMap.Value);

                  storageMethod = SyntaxFactory
                     .MethodDeclaration(SyntaxFactory.ParseTypeName($"Task<{returnValueStr}>"), entry.Name);

                  parameterMethod = parameterMethod
                     .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("key"))
                     .WithType(SyntaxFactory.ParseTypeName(key.ToString())))
                     .AddBodyStatements(SyntaxFactory.ReturnStatement(GetStorageStringRoslyn(storage.Prefix, entry.Name, entry.StorageType, hashers)));

                  storageMethod = storageMethod
                     .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("key"))
                     .WithType(SyntaxFactory.ParseTypeName(key.ToString())));

                  ArgumentListSyntax argumentList = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[] { SyntaxFactory.Argument(SyntaxFactory.IdentifierName("key")) }));

                  methodInvoke = SyntaxFactory.ExpressionStatement(
                     SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName(parameterMethod.Identifier), argumentList));

                  // add storage key mapping in constructor
                  constructor = constructor.AddBodyStatements(AddPropertyValuesRoslyn(GetStorageMapStringRoslyn(key.ToString(), returnValueStr.ToString(), storage.Prefix, entry.Name, hashers), "_client.StorageKeyDict"));
               }
               else
               {
                  throw new NotImplementedException();
               }

               storageMethod = storageMethod
                  .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword)))
                  .WithLeadingTrivia(GetCommentsRoslyn(entry.Docs, null, entry.Name)); // Assuming GetComments() returns a string

               storageMethod = storageMethod
                  .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("token")).WithType(SyntaxFactory.ParseTypeName("CancellationToken")));

               VariableDeclarationSyntax variableDeclaration1 = SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName("string"))
                   .AddVariables(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("parameters"), null, SyntaxFactory.EqualsValueClause(methodInvoke.Expression)));

               storageMethod = storageMethod.AddBodyStatements(SyntaxFactory.LocalDeclarationStatement(variableDeclaration1));

               string resultString = GetInvoceString(returnValueStr.ToString());

               VariableDeclarationSyntax variableDeclaration2 = SyntaxFactory
                  .VariableDeclaration(SyntaxFactory.IdentifierName("var"))
                  .AddVariables(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("result"), null, SyntaxFactory.EqualsValueClause(SyntaxFactory.IdentifierName(resultString))));

               storageMethod = storageMethod.AddBodyStatements(SyntaxFactory.LocalDeclarationStatement(variableDeclaration2));

               storageMethod = storageMethod.AddBodyStatements(SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName("result")));

               // add parameter method to the class
               targetClass = targetClass.AddMembers(parameterMethod);

               // default function
               if (entry.Default != null && entry.Default.Length != 0)
               {
                  string storageDefault = entry.Name + "Default";
                  MethodDeclarationSyntax defaultMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("string"), storageDefault)
                      .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                      .WithLeadingTrivia(GetCommentsRoslyn(new string[] { "Default value as hex string" }, null, storageDefault));

                  // Add return statement
                  defaultMethod = defaultMethod.WithBody(SyntaxFactory.Block(SyntaxFactory.ReturnStatement(
                      SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("0x" + BitConverter.ToString(entry.Default).Replace("-", string.Empty))))));

                  // add default method to the class
                  targetClass = targetClass.AddMembers(defaultMethod);
               }

               // add storage method to the class
               targetClass = targetClass.AddMembers(storageMethod);
            }
         }

         // add constructor to the class
         targetClass = targetClass.AddMembers(constructor);

         // Add class to the namespace.
         typeNamespace = typeNamespace.AddMembers(targetClass);

         return typeNamespace;
      }

      private NamespaceDeclarationSyntax CreateCalls(NamespaceDeclarationSyntax namespaceDeclaration)
      {
         ClassName = Module.Name + "Calls";

         PalletCalls calls = Module.Calls;

         ClassDeclarationSyntax targetClass = SyntaxFactory.ClassDeclaration(ClassName)
             .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.SealedKeyword)));

         if (calls != null)
         {
            if (NodeTypes.TryGetValue(calls.TypeId, out NodeType nodeType))
            {
               var typeDef = nodeType as NodeTypeVariant;

               if (typeDef.Variants != null)
               {
                  foreach (TypeVariant variant in typeDef.Variants)
                  {
                     MethodDeclarationSyntax callMethod = SyntaxFactory
                        .MethodDeclaration(SyntaxFactory.ParseTypeName(nameof(Method)), variant.Name.MakeMethod())
                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                        .WithBody(SyntaxFactory.Block());

                     // add comment to class if exists
                     callMethod = callMethod.WithLeadingTrivia(GetCommentsRoslyn(typeDef.Docs, null, variant.Name));

                     string byteArrayName = "byteArray";

                     TypeSyntax byteListType = SyntaxFactory.ParseTypeName("List<byte>");

                     callMethod = callMethod.AddBodyStatements(
                         SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(
                             byteListType,
                             SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator(
                                 SyntaxFactory.Identifier(byteArrayName),
                                 null,
                                 SyntaxFactory.EqualsValueClause(SyntaxFactory.ObjectCreationExpression(byteListType)
                                     .WithArgumentList(SyntaxFactory.ArgumentList())))))));

                     if (variant.TypeFields != null)
                     {
                        foreach (NodeTypeField field in variant.TypeFields)
                        {
                           NodeTypeResolved fullItem = GetFullItemPath(field.TypeId);

                           // Adding '@' prefix to the parameter
                           string parameterName = EscapeIfKeyword(field.Name);

                           callMethod = callMethod.AddParameterListParameters(
                               SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameterName))
                                   .WithType(SyntaxFactory.ParseTypeName(fullItem.ToString())));

                           callMethod = callMethod.AddBodyStatements(
                               SyntaxFactory.ExpressionStatement(
                                   SyntaxFactory.InvocationExpression(
                                       SyntaxFactory.MemberAccessExpression(
                                           SyntaxKind.SimpleMemberAccessExpression,
                                           SyntaxFactory.IdentifierName(byteArrayName),
                                           SyntaxFactory.IdentifierName("AddRange")),
                                       SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(
                                           SyntaxFactory.Argument(
                                               SyntaxFactory.InvocationExpression(
                                                   SyntaxFactory.MemberAccessExpression(
                                                       SyntaxKind.SimpleMemberAccessExpression,
                                                       SyntaxFactory.IdentifierName(parameterName),
                                                       SyntaxFactory.IdentifierName("Encode")))))))));
                        }
                     }

                     // return statement
                     ObjectCreationExpressionSyntax create = SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName(nameof(Method)))
                         .WithArgumentList(
                             SyntaxFactory.ArgumentList(
                                 SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                     new SyntaxNodeOrToken[]
                                     {
                                        SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal((int)Module.Index))),
                                        SyntaxFactory.Token(SyntaxKind.CommaToken),
                                        SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(Module.Name))),
                                        SyntaxFactory.Token(SyntaxKind.CommaToken),
                                        SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(variant.Index))),
                                        SyntaxFactory.Token(SyntaxKind.CommaToken),
                                        SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(variant.Name))),
                                        SyntaxFactory.Token(SyntaxKind.CommaToken),
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.IdentifierName(byteArrayName),
                                                    SyntaxFactory.IdentifierName("ToArray")))),
                                     })));

                     ReturnStatementSyntax returnStatement = SyntaxFactory.ReturnStatement(create);

                     callMethod = callMethod.AddBodyStatements(returnStatement);
                     targetClass = targetClass.AddMembers(callMethod);
                  }
               }
            }
         }

         namespaceDeclaration = namespaceDeclaration.AddMembers(targetClass);
         return namespaceDeclaration;
      }

      private NamespaceDeclarationSyntax CreateEvents(NamespaceDeclarationSyntax namespaceDeclaration)
      {
         ClassName = Module.Name + "Events";

         PalletEvents events = Module.Events;

         //if (events != null && NodeTypes.TryGetValue(events.TypeId, out NodeType nodeType))
         //{
         //   var typeDef = nodeType as NodeTypeVariant;

         //   if (typeDef.Variants != null)
         //   {
         //      foreach (TypeVariant variant in typeDef.Variants)
         //      {
         //         string eventClassName = "Event" + variant.Name.MakeMethod();
         //         ClassDeclarationSyntax eventClass = SyntaxFactory.ClassDeclaration(eventClassName)
         //             .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.SealedKeyword)))
         //             .WithLeadingTrivia(GetCommentsRoslyn(variant.Docs, null, variant.Name));

         //         QualifiedNameSyntax baseTupleType = SyntaxFactory.QualifiedName(SyntaxFactory.IdentifierName("BaseTuple"), SyntaxFactory.IdentifierName(string.Empty));
         //         if (variant.TypeFields != null)
         //         {
         //            foreach (NodeTypeField field in variant.TypeFields)
         //            {
         //               NodeTypeResolved fullItem = GetFullItemPath(field.TypeId);
         //               baseTupleType = baseTupleType.WithRight(SyntaxFactory.IdentifierName(fullItem.ToString()));
         //            }
         //         }
         //         eventClass = eventClass.AddBaseListTypes(SyntaxFactory.SimpleBaseType(baseTupleType));

         //         namespaceDeclaration = namespaceDeclaration.AddMembers(eventClass);
         //      }
         //   }
         //}

         return namespaceDeclaration;
      }

      private NamespaceDeclarationSyntax CreateConstants(NamespaceDeclarationSyntax namespaceDeclaration)
      {
         ClassName = Module.Name + "Constants";

         PalletConstant[] constants = Module.Constants;

         ClassDeclarationSyntax targetClass = SyntaxFactory.ClassDeclaration(ClassName)
             .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.SealedKeyword)));

         if (constants != null && constants.Any())
         {
            foreach (PalletConstant constant in constants)
            {
               MethodDeclarationSyntax constantMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), constant.Name)
                   .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));

               // add comment to class if exists
               constantMethod = constantMethod.WithLeadingTrivia(GetCommentsRoslyn(constant.Docs, null, constant.Name));

               targetClass = targetClass.AddMembers(constantMethod);

               if (NodeTypes.TryGetValue(constant.TypeId, out NodeType nodeType))
               {
                  NodeTypeResolved nodeTypeResolved = GetFullItemPath(nodeType.Id);
                  constantMethod = constantMethod.WithReturnType(SyntaxFactory.ParseTypeName(nodeTypeResolved.ToString()));

                  // assign new result object
                  constantMethod = constantMethod.AddBodyStatements(
                      SyntaxFactory.LocalDeclarationStatement(
                          SyntaxFactory.VariableDeclaration(
                              SyntaxFactory.IdentifierName("var"),
                              SyntaxFactory.SingletonSeparatedList(
                                  SyntaxFactory.VariableDeclarator(
                                      SyntaxFactory.Identifier("result"),
                                      null,
                                      SyntaxFactory.EqualsValueClause(
                                          SyntaxFactory.ObjectCreationExpression(
                                              SyntaxFactory.ParseTypeName(nodeTypeResolved.ToString()),
                                              SyntaxFactory.ArgumentList(),
                                              null)))))));

                  // create with hex string object
                  constantMethod = constantMethod.AddBodyStatements(
                      SyntaxFactory.ExpressionStatement(
                          SyntaxFactory.InvocationExpression(
                              SyntaxFactory.MemberAccessExpression(
                                  SyntaxKind.SimpleMemberAccessExpression,
                                  SyntaxFactory.IdentifierName("result"),
                                  SyntaxFactory.IdentifierName("Create")),
                              SyntaxFactory.ArgumentList(
                                  SyntaxFactory.SingletonSeparatedList(
                                      SyntaxFactory.Argument(
                                          SyntaxFactory.LiteralExpression(
                                              SyntaxKind.StringLiteralExpression,
                                              SyntaxFactory.Literal("0x" + BitConverter.ToString(constant.Value).Replace("-", string.Empty)))))))));

                  // return statement
                  constantMethod = constantMethod.AddBodyStatements(
                      SyntaxFactory.ReturnStatement(
                          SyntaxFactory.IdentifierName("result")));

                  targetClass = targetClass.ReplaceNode(
                      targetClass.DescendantNodes().OfType<MethodDeclarationSyntax>().Single(m => m.Identifier.Text == constant.Name),
                      constantMethod);
               }
            }
         }

         namespaceDeclaration = namespaceDeclaration.AddMembers(targetClass);
         return namespaceDeclaration;
      }

      private NamespaceDeclarationSyntax CreateErrors(NamespaceDeclarationSyntax namespaceDeclaration)
      {
         ClassName = Module.Name + "Errors";

         PalletErrors errors = Module.Errors;

         if (errors != null)
         {
            if (NodeTypes.TryGetValue(errors.TypeId, out NodeType nodeType))
            {
               var typeDef = nodeType as NodeTypeVariant;

               EnumDeclarationSyntax targetClass = SyntaxFactory.EnumDeclaration(ClassName)
                   .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));

               if (typeDef.Variants != null)
               {
                  foreach (TypeVariant variant in typeDef.Variants)
                  {
                     EnumMemberDeclarationSyntax enumField = SyntaxFactory.EnumMemberDeclaration(variant.Name);

                     // add comment to field if exists
                     enumField = enumField.WithLeadingTrivia(GetCommentsRoslyn(variant.Docs, null, variant.Name));

                     targetClass = targetClass.AddMembers(enumField);
                  }
               }

               namespaceDeclaration = namespaceDeclaration.AddMembers(targetClass);
            }
         }

         return namespaceDeclaration;
      }

      private static string GetInvoceString(string returnType)
      {
         return "await _client.GetStorageAsync<" + returnType + ">(parameters, token)";
      }

      private static InvocationExpressionSyntax GetStorageStringRoslyn(string module, string item, Storage.Type type, Storage.Hasher[] hashers = null)
      {
         var codeExpressions = new List<ArgumentSyntax>
          {
              SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(module))),
              SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(item))),
              SyntaxFactory.Argument(
                  SyntaxFactory.MemberAccessExpression(
                      SyntaxKind.SimpleMemberAccessExpression,
                      SyntaxFactory.IdentifierName("Substrate.NetApi.Model.Meta.Storage.Type"),
                      SyntaxFactory.IdentifierName(type.ToString())))
          };

         // if it is a map fill hashers and key
         if (hashers != null && hashers.Length > 0)
         {
            ExpressionSyntax keyReference = SyntaxFactory.ArrayCreationExpression(
                SyntaxFactory.ArrayType(
                    SyntaxFactory.IdentifierName("Substrate.NetApi.Model.Types.IType"),
                    SyntaxFactory.SingletonList(
                        SyntaxFactory.ArrayRankSpecifier(
                            SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                SyntaxFactory.OmittedArraySizeExpression())))),
                SyntaxFactory.InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                    SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                        SyntaxFactory.IdentifierName("key"))));

            if (hashers.Length > 1)
            {
               keyReference = SyntaxFactory.MemberAccessExpression(
                   SyntaxKind.SimpleMemberAccessExpression,
                   SyntaxFactory.IdentifierName("key"),
                   SyntaxFactory.IdentifierName("Value")
               );
            }

            codeExpressions = new List<ArgumentSyntax>
            {
                SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(module))),
                SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(item))),
                SyntaxFactory.Argument(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName("Substrate.NetApi.Model.Meta.Storage.Type"),
                        SyntaxFactory.IdentifierName(type.ToString()))),
                SyntaxFactory.Argument(
                    SyntaxFactory.ArrayCreationExpression(
                        SyntaxFactory.ArrayType(
                            SyntaxFactory.IdentifierName("Substrate.NetApi.Model.Meta.Storage.Hasher"),
                            SyntaxFactory.SingletonList(SyntaxFactory.ArrayRankSpecifier())),
                        SyntaxFactory.InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                            SyntaxFactory.SeparatedList(
                                hashers.Select(p => SyntaxFactory.ParseExpression($"Substrate.NetApi.Model.Meta.Storage.Hasher.{p}")))))),
                SyntaxFactory.Argument(keyReference)
            };
         }

         return SyntaxFactory.InvocationExpression(
             SyntaxFactory.MemberAccessExpression(
                 SyntaxKind.SimpleMemberAccessExpression,
                 SyntaxFactory.IdentifierName("RequestGenerator"),
                 SyntaxFactory.IdentifierName("GetStorage")))
             .WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(codeExpressions)));
      }

      private static ExpressionSyntax[] GetStorageMapStringRoslyn(string keyType, string returnType, string module, string item, Storage.Hasher[] hashers = null)
      {
         TypeOfExpressionSyntax typeofReturn = SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName(returnType));

         var result = new ExpressionSyntax[] {
              SyntaxFactory.ObjectCreationExpression(
                  SyntaxFactory.IdentifierName("System.Tuple<string,string>"),
                  SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(
                      new ArgumentSyntax[]
                      {
                          SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(module))),
                          SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(item)))
                      })),
                  null),
              SyntaxFactory.ObjectCreationExpression(
                  SyntaxFactory.IdentifierName("System.Tuple<Substrate.NetApi.Model.Meta.Storage.Hasher[], System.Type, System.Type>"),
                  SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(
                      new ArgumentSyntax[]
                      {
                          SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)),
                          SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)),
                          SyntaxFactory.Argument(typeofReturn)
                      })),
                  null)
          };

         // if it is a map fill hashers and key
         if (hashers != null && hashers.Length > 0)
         {
            ArrayCreationExpressionSyntax arrayExpression = SyntaxFactory.ArrayCreationExpression(
                SyntaxFactory.ArrayType(SyntaxFactory.IdentifierName("Substrate.NetApi.Model.Meta.Storage.Hasher[]")),
                SyntaxFactory.InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                    SyntaxFactory.SeparatedList(hashers.Select(p =>
                        SyntaxFactory.ParseExpression($"Substrate.NetApi.Model.Meta.Storage.Hasher.{p}")))));

            TypeOfExpressionSyntax typeofType = SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName(keyType));

            result =
               new ExpressionSyntax[] {
                  SyntaxFactory.ObjectCreationExpression(
                      SyntaxFactory.IdentifierName("System.Tuple<string,string>"),
                      SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(
                          new ArgumentSyntax[]
                          {
                              SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(module))),
                              SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(item)))
                          })),
                      null),
                  SyntaxFactory.ObjectCreationExpression(
                      SyntaxFactory.IdentifierName("System.Tuple<Substrate.NetApi.Model.Meta.Storage.Hasher[], System.Type, System.Type>"),
                      SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(
                          new ArgumentSyntax[]
                          {
                              SyntaxFactory.Argument(arrayExpression),
                              SyntaxFactory.Argument(typeofType),
                              SyntaxFactory.Argument(typeofReturn)
                          })),
                   null)
            };
         }

         return result;
      }

      private static ExpressionStatementSyntax AddPropertyValuesRoslyn(ExpressionSyntax[] exprs, string variableReference)
      {
         return SyntaxFactory.ExpressionStatement(
             SyntaxFactory.InvocationExpression(
                 SyntaxFactory.MemberAccessExpression(
                     SyntaxKind.SimpleMemberAccessExpression,
                     SyntaxFactory.IdentifierName(variableReference),
                     SyntaxFactory.IdentifierName("Add")),
                 SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(exprs.Select(SyntaxFactory.Argument)))));
      }
   }
}
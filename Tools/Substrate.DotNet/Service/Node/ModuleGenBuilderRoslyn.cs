using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Substrate.DotNet.Service.Node.Base;
using Substrate.NetApi.Model.Meta;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;

namespace Substrate.DotNet.Service.Node
{
   public class ModuleGenRoslynBuilder : ModuleBuilderBase
   {
      private ModuleGenRoslynBuilder(string projectName, uint id, PalletModule module, NodeTypeResolver typeDict, Dictionary<uint, NodeType> nodeTypes) :
          base(projectName, id, module, typeDict, nodeTypes)
      {
      }

      public static ModuleGenRoslynBuilder Init(string projectName, uint id, PalletModule module, NodeTypeResolver typeDict, Dictionary<uint, NodeType> nodeTypes)
      {
         return new ModuleGenRoslynBuilder(projectName, id, module, typeDict, nodeTypes);
      }

      public override ModuleGenRoslynBuilder Create()
      {
         ImportsNamespace.Imports.Add(new CodeNamespaceImport("System.Threading.Tasks"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport($"Substrate.NetApi.Model.Meta"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport("System.Threading"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport($"Substrate.NetApi"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport($"Substrate.NetApi.Model.Types"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport($"Substrate.NetApi.Model.Extrinsics"));

         FileName = "Main" + Module.Name;
         NamespaceName = $"{ProjectName}.Generated.Storage";
         ReferenzName = NamespaceName;

         CodeNamespace typeNamespace = new(NamespaceName);
         TargetUnit.Namespaces.Add(typeNamespace);

         // add constructor
         CodeConstructor constructor = new()
         {
            Attributes = MemberAttributes.Public | MemberAttributes.Final
         };

         ConstructorDeclarationSyntax constructorRoslyn = SyntaxFactory.ConstructorDeclaration(SyntaxFactory.Identifier("YourClassName"))
          .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
          .WithBody(SyntaxFactory.Block());
         CreateStorageRoslyn(NamespaceName, constructorRoslyn);

         return this;
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

      private static string GetInvoceString(string returnType)
      {
         return "await _client.GetStorageAsync<" + returnType + ">(parameters, token)";
      }

      private static ExpressionSyntax[] GetStorageMapStringRoslyn(string keyType, string returnType, string module, string item, Storage.Hasher[] hashers = null)
      {
         TypeOfExpressionSyntax typeofReturn = SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName(returnType));

         var result = new ExpressionSyntax[] {
        SyntaxFactory.ObjectCreationExpression(
            SyntaxFactory.IdentifierName("Tuple<string,string>"),
            SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(
                new ArgumentSyntax[]
                {
                    SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(module))),
                    SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(item)))
                })),
            null),
        SyntaxFactory.ObjectCreationExpression(
            SyntaxFactory.IdentifierName("Tuple<Storage.Hasher[], Type, Type>"),
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
                SyntaxFactory.ArrayType(SyntaxFactory.IdentifierName("Storage.Hasher")),
                SyntaxFactory.InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                    SyntaxFactory.SeparatedList(hashers.Select(p =>
                        SyntaxFactory.ParseExpression($"Storage.Hasher.{p}")))));

            TypeOfExpressionSyntax typeofType = SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName(keyType));

            result = new ExpressionSyntax[] {
            SyntaxFactory.ObjectCreationExpression(
                SyntaxFactory.IdentifierName("Tuple<string,string>"),
                SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(
                    new ArgumentSyntax[]
                    {
                        SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(module))),
                        SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(item)))
                    })),
                null),
            SyntaxFactory.ObjectCreationExpression(
                SyntaxFactory.IdentifierName("Tuple<Storage.Hasher[], Type, Type>"),
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

      private static InvocationExpressionSyntax GetStorageStringRoslyn(string module, string item, Storage.Type type, Storage.Hasher[] hashers = null)
      {
         var codeExpressions = new List<ArgumentSyntax>
          {
              SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(module))),
              SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(item))),
              SyntaxFactory.Argument(
                  SyntaxFactory.MemberAccessExpression(
                      SyntaxKind.SimpleMemberAccessExpression,
                      SyntaxFactory.IdentifierName("Storage.Type"),
                      SyntaxFactory.IdentifierName(type.ToString())))
          };

         // if it is a map fill hashers and key
         if (hashers != null && hashers.Length > 0)
         {
            ExpressionSyntax keyReference = SyntaxFactory.ArrayCreationExpression(
                SyntaxFactory.ArrayType(
                    SyntaxFactory.IdentifierName("IType"),
                    SyntaxFactory.SingletonList<ArrayRankSpecifierSyntax>(
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
                        SyntaxFactory.IdentifierName("Storage.Type"),
                        SyntaxFactory.IdentifierName(type.ToString()))),
                SyntaxFactory.Argument(
                    SyntaxFactory.ArrayCreationExpression(
                        SyntaxFactory.ArrayType(
                            SyntaxFactory.IdentifierName("Storage.Hasher"),
                            SyntaxFactory.SingletonList(SyntaxFactory.ArrayRankSpecifier())),
                        SyntaxFactory.InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                            SyntaxFactory.SeparatedList<ExpressionSyntax>(
                                hashers.Select(p => SyntaxFactory.ParseExpression($"Storage.Hasher.{p}")))))),
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

      private void CreateStorageRoslyn(string typeNamespace, ConstructorDeclarationSyntax constructor)
      {
         ClassName = Module.Name + "Storage";

         PalletStorage storage = Module.Storage;

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
             .WithLeadingTrivia(SyntaxFactory.Comment("Substrate client for the storage calls."));

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

         targetClass = targetClass.AddMembers(constructor);

         if (storage?.Entries != null)
         {
            foreach (Entry entry in storage.Entries)
            {
               string storageParams = entry.Name + "Params";

               // Create static method
               MethodDeclarationSyntax parameterMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("string"), storageParams)
                   .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.StaticKeyword), SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                   .WithLeadingTrivia(GetCommentsRoslyn(entry.Docs, null, storageParams)); // Assuming GetComments() returns a string

               targetClass = targetClass.AddMembers(parameterMethod);

               // default function
               if (entry.Default != null && entry.Default.Length != 0)
               {
                  string storageDefault = entry.Name + "Default";
                  MethodDeclarationSyntax defaultMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("string"), storageDefault)
                      .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.StaticKeyword), SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                      .WithLeadingTrivia(GetCommentsRoslyn(new string[] { "Default value as hex string" }, null, storageDefault));

                  // Add return statement
                  defaultMethod = defaultMethod.WithBody(SyntaxFactory.Block(SyntaxFactory.ReturnStatement(
                      SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("0x" + BitConverter.ToString(entry.Default).Replace("-", string.Empty))))));

                  targetClass = targetClass.AddMembers(defaultMethod);
               }

               string returnTypeStr = string.Empty;

               if (entry.StorageType == Storage.Type.Plain)
               {
                  NodeTypeResolved fullItem = GetFullItemPath(entry.TypeMap.Item1);
                  returnTypeStr = $"async Task<{fullItem}>";

                  parameterMethod = parameterMethod.AddBodyStatements(SyntaxFactory.ReturnStatement(GetStorageStringRoslyn(storage.Prefix, entry.Name, entry.StorageType)));

                  // add storage key mapping in constructor
                  constructor = constructor.AddBodyStatements(AddPropertyValuesRoslyn(GetStorageMapStringRoslyn("", fullItem.ToString(), storage.Prefix, entry.Name), "_client.StorageKeyDict"));
               }
               else if (entry.StorageType == Storage.Type.Map)
               {
                  TypeMap typeMap = entry.TypeMap.Item2;
                  Storage.Hasher[] hashers = typeMap.Hashers;
                  NodeTypeResolved key = GetFullItemPath(typeMap.Key);
                  NodeTypeResolved value = GetFullItemPath(typeMap.Value);

                  returnTypeStr = $"async Task<{value}>";

                  parameterMethod = parameterMethod.AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("key")).WithType(SyntaxFactory.ParseTypeName(key.ToString())))
                      .AddBodyStatements(SyntaxFactory.ReturnStatement(GetStorageStringRoslyn(storage.Prefix, entry.Name, entry.StorageType, hashers)));

                  // add storage key mapping in constructor
                  constructor = constructor.AddBodyStatements(AddPropertyValuesRoslyn(GetStorageMapStringRoslyn(key.ToString(), value.ToString(), storage.Prefix, entry.Name, hashers), "_client.StorageKeyDict"));
               }
               else
               {
                  throw new NotImplementedException();
               }

               MethodDeclarationSyntax storageMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(returnTypeStr), entry.Name)
                   .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                   .WithLeadingTrivia(GetCommentsRoslyn(entry.Docs, null, entry.Name)); // Assuming GetComments() returns a string

               storageMethod = storageMethod.AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("token")).WithType(SyntaxFactory.ParseTypeName("CancellationToken")));

               ExpressionStatementSyntax methodInvoke = SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName(parameterMethod.Identifier)));

               VariableDeclarationSyntax variableDeclaration1 = SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName("string"))
                   .AddVariables(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("parameters"), null, SyntaxFactory.EqualsValueClause(methodInvoke.Expression)));

               storageMethod = storageMethod.AddBodyStatements(SyntaxFactory.LocalDeclarationStatement(variableDeclaration1));

               string resultString = GetInvoceString(returnTypeStr.Replace("async Task<", "").Replace(">", ""));
               VariableDeclarationSyntax variableDeclaration2 = SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var"))
                   .AddVariables(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("result"), null, SyntaxFactory.EqualsValueClause(SyntaxFactory.IdentifierName(resultString))));

               storageMethod = storageMethod.AddBodyStatements(SyntaxFactory.LocalDeclarationStatement(variableDeclaration2));

               storageMethod = storageMethod.AddBodyStatements(SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName("result")));

               targetClass = targetClass.AddMembers(storageMethod);
            }
         }

         // Add class to the namespace.
         NamespaceDeclarationSyntax targetNamespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(typeNamespace))
             .AddMembers(targetClass);

         // Use the Roslyn SyntaxTree object to create the final tree.
         SyntaxTree tree = SyntaxFactory.SyntaxTree(targetNamespace);
      }

      public static SyntaxTriviaList GetCommentsRoslyn(string[] docs, NodeType typeDef = null, string typeName = null)
      {
         var commentList = new List<SyntaxTrivia>
         {
            SyntaxFactory.Comment("/// <summary>")
         };

         if (typeDef != null)
         {
            string path = typeDef.Path != null ? "[" + string.Join('.', typeDef.Path) + "]" : "";
            commentList.Add(SyntaxFactory.Comment($"/// >> {typeDef.Id} - {typeDef.TypeDef}{path}"));
         }

         if (typeName != null)
         {
            commentList.Add(SyntaxFactory.Comment($"/// >> {typeName}"));
         }

         if (docs != null)
         {
            foreach (string doc in docs)
            {
               commentList.Add(SyntaxFactory.Comment($"/// {doc}"));
            }
         }

         commentList.Add(SyntaxFactory.Comment("/// </summary>"));

         return SyntaxFactory.TriviaList(commentList);
      }
   }
}
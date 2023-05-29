using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Substrate.DotNet.Extensions;
using Substrate.DotNet.Service.Node.Base;
using Substrate.NetApi.Model.Meta;
using System.Collections.Generic;
using System.Linq;

namespace Substrate.DotNet.Service.Node
{
   public class StructBuilderRoslyn : TypeBuilderBaseRoslyn
   {
      private StructBuilderRoslyn(string projectName, uint id, NodeTypeComposite typeDef, NodeTypeResolver typeDict)
          : base(projectName, id, typeDef, typeDict)
      {
      }

      private static FieldDeclarationSyntax GetPropertyFieldRoslyn(string name, string baseType)
      {
         FieldDeclarationSyntax fieldDeclaration = SyntaxFactory.FieldDeclaration(
             SyntaxFactory.VariableDeclaration(
                 SyntaxFactory.ParseTypeName(baseType),
                 SyntaxFactory.SingletonSeparatedList(
                     SyntaxFactory.VariableDeclarator(name.MakePrivateField()))))
             .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));

         return fieldDeclaration;
      }

      private static PropertyDeclarationSyntax GetPropertyWithFieldRoslyn(string name, FieldDeclarationSyntax propertyField)
      {
         string propertyName = name.MakeMethod();
         TypeSyntax propertyType = propertyField.Declaration.Type;

         PropertyDeclarationSyntax prop = SyntaxFactory.PropertyDeclaration(propertyType, propertyName)
             .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
             .AddAccessorListAccessors(
                 SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                     .WithBody(SyntaxFactory.Block(SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName(propertyField.Declaration.Variables[0].Identifier)))),
                 SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                     .WithBody(SyntaxFactory.Block(
                         SyntaxFactory.ExpressionStatement(
                             SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                 SyntaxFactory.IdentifierName(propertyField.Declaration.Variables[0].Identifier),
                                 SyntaxFactory.IdentifierName("value"))))));

         return prop;
      }

      private static PropertyDeclarationSyntax GetPropertyRoslyn(string name, TypeSyntax type)
      {
         string propertyName = name.MakeMethod();

         PropertyDeclarationSyntax prop = SyntaxFactory.PropertyDeclaration(type, propertyName)
             .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
             .AddAccessorListAccessors(
                 SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                     .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                 SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                     .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));

         return prop;
      }

      private MethodDeclarationSyntax GetDecodeRoslyn(NodeTypeField[] typeFields)
      {
         MethodDeclarationSyntax decodeMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "Decode")
             .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
             .AddModifiers(SyntaxFactory.Token(SyntaxKind.OverrideKeyword))
             .AddParameterListParameters(
                 SyntaxFactory.Parameter(SyntaxFactory.Identifier("byteArray")).WithType(SyntaxFactory.ParseTypeName("byte[]")),
                 SyntaxFactory.Parameter(SyntaxFactory.Identifier("p")).WithType(SyntaxFactory.ParseTypeName("int")).WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.RefKeyword))))
             .WithBody(SyntaxFactory.Block());

         decodeMethod = decodeMethod.AddBodyStatements(SyntaxFactory.ParseStatement("var start = p;"));

         if (typeFields != null)
         {
            for (int i = 0; i < typeFields.Length; i++)
            {
               NodeTypeField typeField = typeFields[i];

               string fieldName = GetFieldName(typeField, "value", typeFields.Length, i);
               NodeTypeResolved fullItem = GetFullItemPath(typeField.TypeId);

               decodeMethod = decodeMethod.AddBodyStatements(SyntaxFactory.ParseStatement($"{fieldName.MakeMethod()} = new {fullItem}();"));
               decodeMethod = decodeMethod.AddBodyStatements(SyntaxFactory.ParseStatement($"{fieldName.MakeMethod()}.Decode(byteArray, ref p);"));
            }
         }

         decodeMethod = decodeMethod.AddBodyStatements(
             SyntaxFactory.ParseStatement("var bytesLength = p - start;"),
             SyntaxFactory.ParseStatement("TypeSize = bytesLength;"),
             SyntaxFactory.ParseStatement("Bytes = new byte[bytesLength];"),
             SyntaxFactory.ParseStatement("System.Array.Copy(byteArray, start, Bytes, 0, bytesLength);")
         );

         return decodeMethod;
      }

      private MethodDeclarationSyntax GetEncodeRoslyn(NodeTypeField[] typeFields)
      {
         MethodDeclarationSyntax encodeMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("System.Byte[]"), "Encode")
             .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.OverrideKeyword))
             .WithBody(SyntaxFactory.Block());

         encodeMethod = encodeMethod.AddBodyStatements(SyntaxFactory.ParseStatement("var result = new List<byte>();"));

         if (typeFields != null)
         {
            for (int i = 0; i < typeFields.Length; i++)
            {
               NodeTypeField typeField = typeFields[i];
               string fieldName = StructBuilderRoslyn.GetFieldName(typeField, "value", typeFields.Length, i);

               encodeMethod = encodeMethod.AddBodyStatements(SyntaxFactory.ParseStatement($"result.AddRange({fieldName.MakeMethod()}.Encode());"));
            }
         }

         encodeMethod = encodeMethod.AddBodyStatements(SyntaxFactory.ParseStatement("return result.ToArray();"));

         return encodeMethod;
      }

      public static BuilderBaseRoslyn Init(string projectName, uint id, NodeTypeComposite typeDef, NodeTypeResolver typeDict)
      {
         return new StructBuilderRoslyn(projectName, id, typeDef, typeDict);
      }

      public override TypeBuilderBaseRoslyn Create()
      {
         var typeDef = TypeDef as NodeTypeComposite;

         ClassName = $"{typeDef.Path.Last()}";

         ReferenzName = $"{NamespaceName}.{ClassName}";

         ClassDeclarationSyntax targetClass = SyntaxFactory.ClassDeclaration(ClassName)
             .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.SealedKeyword))
             .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.IdentifierName("BaseType")));

         targetClass = AddTargetClassCustomAttributesRoslyn(targetClass, typeDef);
         // add comment to class if exists
         targetClass = targetClass.WithLeadingTrivia(GetCommentsRoslyn(typeDef.Docs, typeDef));

         if (typeDef.TypeFields != null)
         {
            for (int i = 0; i < typeDef.TypeFields.Length; i++)
            {
               NodeTypeField typeField = typeDef.TypeFields[i];
               string fieldName = GetFieldName(typeField, "value", typeDef.TypeFields.Length, i);

               NodeTypeResolved fullItem = GetFullItemPath(typeField.TypeId);

               //FieldDeclarationSyntax field = GetPropertyFieldRoslyn(fieldName, fullItem.ToString());
               // add comment to field if exists
               //field = field.WithLeadingTrivia(GetCommentsRoslyn(typeField.Docs, null, fieldName));
               //targetClass = targetClass.AddMembers(field, GetPropertyWithFieldRoslyn(fieldName, field));

               PropertyDeclarationSyntax propertyDeclaration = GetPropertyRoslyn(fieldName, SyntaxFactory.ParseTypeName(fullItem.ToString()));
               propertyDeclaration = propertyDeclaration.WithLeadingTrivia(GetCommentsRoslyn(typeField.Docs, null, fieldName));

               targetClass = targetClass.AddMembers(propertyDeclaration);
            }
         }

         MethodDeclarationSyntax nameMethod = SimpleMethodRoslyn("TypeName", "System.String", ClassName);
         targetClass = targetClass.AddMembers(nameMethod);

         MethodDeclarationSyntax encodeMethod = GetEncodeRoslyn(typeDef.TypeFields);
         targetClass = targetClass.AddMembers(encodeMethod);

         MethodDeclarationSyntax decodeMethod = GetDecodeRoslyn(typeDef.TypeFields);
         targetClass = targetClass.AddMembers(decodeMethod);

         NamespaceDeclarationSyntax namespaceDeclaration = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.IdentifierName(NamespaceName))
             .AddMembers(targetClass);

         CompilationUnitSyntax compilationUnit = SyntaxFactory.CompilationUnit()
             .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System")))
             .AddMembers(namespaceDeclaration);

         TargetUnit = TargetUnit.AddMembers(compilationUnit.Members.ToArray());

         return this;
      }

      private static string GetFieldName(NodeTypeField typeField, string alterName, int length, int index)
      {
         if (typeField.Name == null)
         {
            if (length > 1)
            {
               if (typeField.TypeName == null)
               {
                  return alterName + index;
               }
               else
               {
                  return typeField.TypeName;
               }
            }
            else
            {
               return alterName;
            }
         }
         else
         {
            return typeField.Name;
         }
      }
   }
}
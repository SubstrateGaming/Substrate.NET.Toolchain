using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Substrate.DotNet.Service.Node.Base;
using Substrate.NetApi.Model.Meta;
using System;
using System.CodeDom;
using System.Linq;
using System.Reflection;

namespace Substrate.DotNet.Service.Node
{
   public class ArrayBuilderRoslyn : TypeBuilderBaseRoslyn
   {
      public static int Counter = 0;

      private ArrayBuilderRoslyn(string projectName, uint id, NodeTypeArray typeDef, NodeTypeResolver typeDict)
          : base(projectName, id, typeDef, typeDict)
      {
      }

      private static MethodDeclarationSyntax GetDecodeRoslyn(string baseType)
      {
         MethodDeclarationSyntax decodeMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "Decode")
             .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
             .AddModifiers(SyntaxFactory.Token(SyntaxKind.OverrideKeyword))
             .AddParameterListParameters(
                 SyntaxFactory.Parameter(SyntaxFactory.Identifier("byteArray")).WithType(SyntaxFactory.ParseTypeName("byte[]")),
                 SyntaxFactory.Parameter(SyntaxFactory.Identifier("p")).WithType(SyntaxFactory.ParseTypeName("int")).WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.RefKeyword))))
             .WithBody(SyntaxFactory.Block());

         decodeMethod = decodeMethod.AddBodyStatements(SyntaxFactory.ParseStatement("var start = p;"));

         decodeMethod = decodeMethod.AddBodyStatements(
            SyntaxFactory.ParseStatement($"var array = new {baseType}[TypeSize];"));

         decodeMethod = decodeMethod.AddBodyStatements(
            SyntaxFactory.ParseStatement("for (var i = 0; i < array.Length; i++) " +
             "{" +
             $"var t = new {baseType}();" +
             "t.Decode(byteArray, ref p);" +
             "array[i] = t;" +
             "}"));

         decodeMethod = decodeMethod.AddBodyStatements(
            SyntaxFactory.ParseStatement("var bytesLength = p - start;"));
         decodeMethod = decodeMethod.AddBodyStatements(
            SyntaxFactory.ParseStatement("Bytes = new byte[bytesLength];"));
         decodeMethod = decodeMethod.AddBodyStatements(
            SyntaxFactory.ParseStatement("System.Array.Copy(byteArray, start, Bytes, 0, bytesLength);"));
         decodeMethod = decodeMethod.AddBodyStatements(
            SyntaxFactory.ParseStatement("Value = array;"));
         return decodeMethod;
      }

      private static MethodDeclarationSyntax GetEncodeRoslyn()
      {
         MethodDeclarationSyntax encodeMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("System.Byte[]"), "Encode")
             .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.OverrideKeyword))
             .WithBody(SyntaxFactory.Block());

         encodeMethod = encodeMethod
            .AddBodyStatements(SyntaxFactory.ParseStatement("var result = new List<byte>();"));

         encodeMethod = encodeMethod.AddBodyStatements(
            SyntaxFactory.ParseStatement("foreach (var v in Value)" +
               "{" +
               "result.AddRange(v.Encode());" +
               "}"));

         encodeMethod = encodeMethod.AddBodyStatements(SyntaxFactory.ParseStatement("return result.ToArray();"));

         return encodeMethod;
      }

      public static ArrayBuilderRoslyn Create(string projectName, uint id, NodeTypeArray nodeType, NodeTypeResolver typeDict)
      {
         return new ArrayBuilderRoslyn(projectName, id, nodeType, typeDict);
      }

      public override TypeBuilderBaseRoslyn Create()
      {
         var typeDef = TypeDef as NodeTypeArray;

         NodeTypeResolved fullItem = GetFullItemPath(typeDef.TypeId);

         ClassName = $"Arr{typeDef.Length}{fullItem.ClassName}";

         if (ClassName.Any(ch => !char.IsLetterOrDigit(ch)))
         {
            // TODO: check if this is a valid solution
            Counter++;
            ClassName = $"Arr{typeDef.Length}Special" + Counter++;
         }

         ReferenzName = $"{NamespaceName}.{ClassName}";

         ClassDeclarationSyntax targetClass = SyntaxFactory.ClassDeclaration(ClassName)
             .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.SealedKeyword))
             .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("BaseType")));

         //FieldDeclarationSyntax valueField = SyntaxFactory.FieldDeclaration(SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName($"{fullItem}[]"))
         //        .AddVariables(SyntaxFactory.VariableDeclarator("_value")))
         //    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
         //targetClass = targetClass.AddMembers(valueField);

         PropertyDeclarationSyntax valueProperty = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName($"{fullItem}[]"), "Value")
             .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
             .AddAccessorListAccessors(
                 SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                 SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
         targetClass = targetClass.AddMembers(valueProperty);


         ReturnStatementSyntax typeNameReturn = SyntaxFactory.ReturnStatement(
             SyntaxFactory.InvocationExpression(
                 SyntaxFactory.MemberAccessExpression(
                     SyntaxKind.SimpleMemberAccessExpression,
                     SyntaxFactory.ParseTypeName("string"),
                     SyntaxFactory.IdentifierName("Format")),
                 SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[]
                 {
            SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("[{0}; {1}]"))),
            SyntaxFactory.Argument(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName(fullItem.ToString())).WithArgumentList(SyntaxFactory.ArgumentList()),
                        SyntaxFactory.IdentifierName("TypeName")))),
            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("TypeSize"))
                 }))));

         // Declaring a name method
         MethodDeclarationSyntax nameMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("string"), "TypeName")
             .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.OverrideKeyword))
             .WithBody(SyntaxFactory.Block(typeNameReturn));
         targetClass = targetClass.AddMembers(nameMethod);


         targetClass = AddTargetClassCustomAttributesRoslyn(targetClass, typeDef);
         // add comment to class if exists
         targetClass = targetClass.WithLeadingTrivia(GetCommentsRoslyn(typeDef.Docs, typeDef));

         targetClass = targetClass.AddMembers(GetEncodeRoslyn(), GetDecodeRoslyn(fullItem.ToString()));

         MethodDeclarationSyntax createMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "Create")
             .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
             .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("array")).WithType(SyntaxFactory.ParseTypeName($"{fullItem}[]")))
             .WithBody(SyntaxFactory.Block(
                 SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SyntaxFactory.IdentifierName("Value"), SyntaxFactory.IdentifierName("array"))),
                 SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SyntaxFactory.IdentifierName("Bytes"), SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName("Encode"))))));

         targetClass = targetClass.AddMembers(createMethod);

         NamespaceDeclarationSyntax namespaceDeclaration = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName(NamespaceName))
            .AddMembers(targetClass);

         CompilationUnitSyntax compilationUnit = SyntaxFactory.CompilationUnit()
            .AddMembers(namespaceDeclaration);

         TargetUnit = TargetUnit.AddMembers(compilationUnit.Members.ToArray());

         return this;
      }

   }
}
using Substrate.DotNet.Service.Node.Base;
using Substrate.NetApi.Model.Meta;
using Substrate.NetApi.Model.Types.Metadata.V14;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Substrate.DotNet.Service.Node
{
   public class ArrayBuilder : TypeBuilderBase
   {
      public static int Counter = 0;
      private ArrayBuilder(string projectName, uint id, NodeTypeArray typeDef, NodeTypeResolver typeDict)
          : base(projectName, id, typeDef, typeDict)
      {
      }

      private static CodeMemberMethod GetDecode(string baseType)
      {
         CodeMemberMethod memberMethod = SimpleMethod("Decode");
         CodeParameterDeclarationExpression param1 = new()
         {
            Type = new CodeTypeReference("System.Byte[]"),
            Name = "byteArray"
         };
         memberMethod.Comments.Add(new CodeCommentStatement("<inheritdoc/>", true));
         memberMethod.Parameters.Add(param1);
         CodeParameterDeclarationExpression param2 = new()
         {
            Type = new CodeTypeReference("System.Int32"),
            Name = "p",
            Direction = FieldDirection.Ref
         };
         memberMethod.Parameters.Add(param2);
         memberMethod.Statements.Add(new CodeSnippetExpression("var start = p"));
         memberMethod.Statements.Add(new CodeSnippetExpression($"var array = new {baseType}[TypeSize]"));
         memberMethod.Statements.Add(new CodeSnippetExpression("for (var i = 0; i < array.Length; i++) " +
             "{" +
             $"var t = new {baseType}();" +
             "t.Decode(byteArray, ref p);" +
             "array[i] = t;" +
             "}"));
         memberMethod.Statements.Add(new CodeSnippetExpression("var bytesLength = p - start"));
         memberMethod.Statements.Add(new CodeSnippetExpression("Bytes = new byte[bytesLength]"));
         memberMethod.Statements.Add(new CodeSnippetExpression("System.Array.Copy(byteArray, start, Bytes, 0, bytesLength)"));
         memberMethod.Statements.Add(new CodeSnippetExpression("Value = array"));
         return memberMethod;
      }

      private static CodeMemberMethod GetEncode()
      {
         CodeMemberMethod memberMethod = new()
         {
            Attributes = MemberAttributes.Public | MemberAttributes.Override,
            Name = "Encode",
            ReturnType = new CodeTypeReference("System.Byte[]")
         };
         memberMethod.Comments.Add(new CodeCommentStatement("<inheritdoc/>", true));
         memberMethod.Statements.Add(new CodeSnippetExpression("var result = new List<byte>()"));
         memberMethod.Statements.Add(new CodeSnippetExpression("foreach (var v in Value)" +
             "{" +
             "result.AddRange(v.Encode());" +
             "}"));
         memberMethod.Statements.Add(new CodeSnippetExpression("return result.ToArray()"));
         return memberMethod;
      }

      public static ArrayBuilder Create(string projectName, uint id, NodeTypeArray nodeType, NodeTypeResolver typeDict)
      {
         return new ArrayBuilder(projectName, id, nodeType, typeDict);
      }

      public override TypeBuilderBase Create()
      {
         var typeDef = TypeDef as NodeTypeArray;

         NodeTypeResolved fullItem = GetFullItemPath(typeDef.TypeId);

         ClassName = $"Arr{typeDef.Length}{fullItem.ClassName}";

         CodeNamespace typeNamespace = new(NamespaceName);
         TargetUnit.Namespaces.Add(typeNamespace);

         if (ClassName.Any(ch => !char.IsLetterOrDigit(ch)))
         {
            Counter++;
            ClassName = $"Arr{typeDef.Length}Special" + Counter++;
         }

         ReferenzName = $"{NamespaceName}.{ClassName}";

         var targetClass = new CodeTypeDeclaration(ClassName)
         {
            IsClass = true,
            TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed
         };
         targetClass.BaseTypes.Add(new CodeTypeReference("BaseType"));

         // add comment to class if exists
         targetClass.Comments.AddRange(GetComments(typeDef.Docs, typeDef));
         AddTargetClassCustomAttributes(targetClass, typeDef);

         typeNamespace.Types.Add(targetClass);

         // Declaring a name method
         CodeMemberMethod nameMethod = new()
         {
            Attributes = MemberAttributes.Public | MemberAttributes.Override,
            Name = "TypeName",
            ReturnType = new CodeTypeReference(typeof(string))
         };
         nameMethod.Comments.Add(new CodeCommentStatement("<inheritdoc/>", true));

         var methodRef1 = new CodeMethodReferenceExpression(new CodeObjectCreateExpression(fullItem.ToString(), Array.Empty<CodeExpression>()), "TypeName()");
         var methodRef2 = new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "TypeSize");

         // Declaring a return statement for method ToString.
         CodeMethodReturnStatement returnStatement =
             new()
             {
                Expression =
                     new CodeMethodInvokeExpression(
                     new CodeTypeReferenceExpression("System.String"), "Format",
                     new CodePrimitiveExpression("[{0}; {1}]"),
                     methodRef1, methodRef2)
             };
         nameMethod.Statements.Add(returnStatement);
         targetClass.Members.Add(nameMethod);

         CodeMemberProperty sizeProperty = new()
         {
            Attributes = MemberAttributes.Public | MemberAttributes.Override,
            Name = "TypeSize",
            Type = new CodeTypeReference(typeof(int))
         };
         sizeProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression((int)typeDef.Length)));
         sizeProperty.Comments.Add(new CodeCommentStatement("<inheritdoc/>", true));
         targetClass.Members.Add(sizeProperty);


         CodeMemberMethod encodeMethod = ArrayBuilder.GetEncode();
         targetClass.Members.Add(encodeMethod);

         CodeMemberMethod decodeMethod = ArrayBuilder.GetDecode(fullItem.ToString());
         targetClass.Members.Add(decodeMethod);

         CodeMemberMethod createMethod = new()
         {
            Attributes = MemberAttributes.Public | MemberAttributes.Final,
            Name = "Create"
         };
         createMethod.Comments.Add(new CodeCommentStatement("<inheritdoc/>", true));
         createMethod.Parameters.Add(new()
         {
            Type = new CodeTypeReference($"{fullItem}[]"),
            Name = "array"
         });
         createMethod.Statements.Add(new CodeSnippetExpression("Value = array"));
         createMethod.Statements.Add(new CodeSnippetExpression("Bytes = Encode()"));
         targetClass.Members.Add(createMethod);

         CodeTypeMember valueProperty = CreateAutoProperty("Value", fullItem.ToString());
         valueProperty.Comments.AddRange(GetComments(null, null, $"{fullItem}[]"));
         targetClass.Members.Add(valueProperty);

         return this;
      }

      public static CodeTypeMember CreateAutoProperty(string propertyName, string propertyType)
      {
         // Ensure the property name starts with a capital letter
         string formattedPropertyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(propertyName);

         // Property declaration for an array type
         string propertyCode = $"        public {propertyType}[] {formattedPropertyName} {{ get; set; }}";

         // Create the CodeSnippetTypeMember
         CodeSnippetTypeMember autoProperty = new(propertyCode);

         return autoProperty;
      }
   }
}

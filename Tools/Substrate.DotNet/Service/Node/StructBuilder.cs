using Substrate.DotNet.Extensions;
using Substrate.DotNet.Service.Node.Base;
using Substrate.NetApi.Model.Meta;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Substrate.DotNet.Service.Node
{
   public class StructBuilder : TypeBuilderBase
   {
      private StructBuilder(string projectName, uint id, NodeTypeComposite typeDef, NodeTypeResolver typeDict)
          : base(projectName, id, typeDef, typeDict)
      {
      }

      private static CodeMemberField GetPropertyField(string name, string baseType)
      {
         CodeMemberField field = new()
         {
            Attributes = MemberAttributes.Private,
            Name = name.MakePrivateField(),
            Type = new CodeTypeReference($"{baseType}")
         };
         return field;
      }

      private static CodeMemberProperty GetProperty(string name, CodeMemberField propertyField)
      {
         CodeMemberProperty prop = new()
         {
            Attributes = MemberAttributes.Public | MemberAttributes.Final,
            Name = name.MakeMethod(),
            HasGet = true,
            HasSet = true,
            Type = propertyField.Type
         };
         prop.GetStatements.Add(new CodeMethodReturnStatement(
             new CodeFieldReferenceExpression(
             new CodeThisReferenceExpression(), propertyField.Name)));
         prop.SetStatements.Add(new CodeAssignStatement(
             new CodeFieldReferenceExpression(
                 new CodeThisReferenceExpression(), propertyField.Name),
                 new CodePropertySetValueReferenceExpression()));
         return prop;
      }

      private CodeMemberMethod GetDecode(NodeTypeField[] typeFields)
      {
         CodeMemberMethod decodeMethod = SimpleMethod("Decode");
         CodeParameterDeclarationExpression param1 = new()
         {
            Type = new CodeTypeReference("System.Byte[]"),
            Name = "byteArray"
         };
         decodeMethod.Parameters.Add(param1);
         CodeParameterDeclarationExpression param2 = new()
         {
            Type = new CodeTypeReference("System.Int32"),
            Name = "p",
            Direction = FieldDirection.Ref
         };
         decodeMethod.Parameters.Add(param2);
         decodeMethod.Statements.Add(new CodeSnippetExpression("var start = p"));

         if (typeFields != null)
         {
            for (int i = 0; i < typeFields.Length; i++)
            {
               NodeTypeField typeField = typeFields[i];

               string fieldName = StructBuilder.GetFieldName(typeField, "value", typeFields.Length, i);
               NodeTypeResolved fullItem = GetFullItemPath(typeField.TypeId);

               decodeMethod.Statements.Add(new CodeSnippetExpression($"{fieldName.MakeMethod()} = new {fullItem.ToString()}()"));
               decodeMethod.Statements.Add(new CodeSnippetExpression($"{fieldName.MakeMethod()}.Decode(byteArray, ref p)"));
            }
         }
         decodeMethod.Statements.Add(new CodeSnippetExpression("var bytesLength = p - start"));
         decodeMethod.Statements.Add(new CodeSnippetExpression("TypeSize = bytesLength"));
         decodeMethod.Statements.Add(new CodeSnippetExpression("Bytes = new byte[bytesLength]"));
         decodeMethod.Statements.Add(new CodeSnippetExpression("System.Array.Copy(byteArray, start, Bytes, 0, bytesLength)"));

         return decodeMethod;
      }

      private static CodeMemberMethod GetEncode(NodeTypeField[] typeFields)
      {
         CodeMemberMethod encodeMethod = new()
         {
            Attributes = MemberAttributes.Public | MemberAttributes.Override,
            Name = "Encode",
            ReturnType = new CodeTypeReference("System.Byte[]")
         };
         encodeMethod.Statements.Add(new CodeSnippetExpression("var result = new List<byte>()"));

         if (typeFields != null)
         {
            for (int i = 0; i < typeFields.Length; i++)
            {
               NodeTypeField typeField = typeFields[i];
               string fieldName = StructBuilder.GetFieldName(typeField, "value", typeFields.Length, i);

               encodeMethod.Statements.Add(new CodeSnippetExpression($"result.AddRange({fieldName.MakeMethod()}.Encode())"));
            }
         }

         encodeMethod.Statements.Add(new CodeSnippetExpression("return result.ToArray()"));
         return encodeMethod;
      }

      public static BuilderBase Init(string projectName, uint id, NodeTypeComposite typeDef, NodeTypeResolver typeDict)
      {
         return new StructBuilder(projectName, id, typeDef, typeDict);
      }

      public override TypeBuilderBase Create()
      {
         var typeDef = TypeDef as NodeTypeComposite;

         ClassName = $"{typeDef.Path.Last()}";

         ReferenzName = $"{NamespaceName}.{typeDef.Path.Last()}";

         CodeNamespace typeNamespace = new(NamespaceName);
         TargetUnit.Namespaces.Add(typeNamespace);

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

         CodeMemberMethod nameMethod = SimpleMethod("TypeName", "System.String", ClassName);
         targetClass.Members.Add(nameMethod);

         if (typeDef.TypeFields != null)
         {
            for (int i = 0; i < typeDef.TypeFields.Length; i++)
            {

               NodeTypeField typeField = typeDef.TypeFields[i];
               string fieldName = StructBuilder.GetFieldName(typeField, "value", typeDef.TypeFields.Length, i);

               NodeTypeResolved fullItem = GetFullItemPath(typeField.TypeId);

               CodeMemberField field = StructBuilder.GetPropertyField(fieldName, fullItem.ToString());

               // add comment to field if exists
               field.Comments.AddRange(GetComments(typeField.Docs, null, fieldName));

               targetClass.Members.Add(field);
               targetClass.Members.Add(StructBuilder.GetProperty(fieldName, field));
            }
         }

         CodeMemberMethod encodeMethod = StructBuilder.GetEncode(typeDef.TypeFields);
         targetClass.Members.Add(encodeMethod);

         CodeMemberMethod decodeMethod = GetDecode(typeDef.TypeFields);
         targetClass.Members.Add(decodeMethod);

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

using Substrate.DotNet.Extensions;
using Substrate.DotNet.Service.Node.Base;
using Substrate.NetApi.Model.Meta;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Substrate.DotNet.Service.Node
{
   public class StructBuilder : TypeBuilderBase
   {
      private StructBuilder(string projectName, uint id, NodeTypeComposite typeDef, NodeTypeResolver typeDict)
          : base(projectName, id, typeDef, typeDict)
      {
      }

      public CodeTypeMember CreateAutoProperty(string propertyName, string propertyType)
      {
         string propertyCode = $"        public {propertyType} {propertyName.MakeMethod()} {{ get; set; }}";

         // Using CodeSnippetTypeMember to inject a raw string of code.
         CodeSnippetTypeMember autoProperty = new(propertyCode);
         return autoProperty;
      }

      private CodeMemberMethod GetDecode(NodeTypeField[] typeFields)
      {
         CodeMemberMethod memberMethod = SimpleMethod("Decode");
         memberMethod.Comments.Add(new CodeCommentStatement("<inheritdoc/>", true));

         CodeParameterDeclarationExpression param1 = new()
         {
            Type = new CodeTypeReference("System.Byte[]"),
            Name = "byteArray"
         };
         memberMethod.Parameters.Add(param1);
         CodeParameterDeclarationExpression param2 = new()
         {
            Type = new CodeTypeReference("System.Int32"),
            Name = "p",
            Direction = FieldDirection.Ref
         };
         memberMethod.Parameters.Add(param2);
         memberMethod.Statements.Add(new CodeSnippetExpression("var start = p"));

         if (typeFields != null)
         {
            for (int i = 0; i < typeFields.Length; i++)
            {
               NodeTypeField typeField = typeFields[i];

               string fieldName = StructBuilder.GetFieldName(typeField, "value", typeFields.Length, i);
               NodeTypeResolved fullItem = GetFullItemPath(typeField.TypeId);

               memberMethod.Statements.Add(new CodeSnippetExpression($"{fieldName.MakeMethod()} = new {fullItem.ToString()}()"));
               memberMethod.Statements.Add(new CodeSnippetExpression($"{fieldName.MakeMethod()}.Decode(byteArray, ref p)"));
            }
         }
         memberMethod.Statements.Add(new CodeSnippetExpression("var bytesLength = p - start"));
         memberMethod.Statements.Add(new CodeSnippetExpression("TypeSize = bytesLength"));
         memberMethod.Statements.Add(new CodeSnippetExpression("Bytes = new byte[bytesLength]"));
         memberMethod.Statements.Add(new CodeSnippetExpression("global::System.Array.Copy(byteArray, start, Bytes, 0, bytesLength)"));

         return memberMethod;
      }

      private static CodeMemberMethod GetEncode(NodeTypeField[] typeFields)
      {
         CodeMemberMethod memberMethod = new()
         {
            Attributes = MemberAttributes.Public | MemberAttributes.Override,
            Name = "Encode",
            ReturnType = new CodeTypeReference("System.Byte[]"),
         };
         memberMethod.Comments.Add(new CodeCommentStatement("<inheritdoc/>", true));
         memberMethod.Statements.Add(new CodeSnippetExpression("var result = new List<byte>()"));

         if (typeFields != null)
         {
            for (int i = 0; i < typeFields.Length; i++)
            {
               NodeTypeField typeField = typeFields[i];
               string fieldName = StructBuilder.GetFieldName(typeField, "value", typeFields.Length, i);

               memberMethod.Statements.Add(new CodeSnippetExpression($"result.AddRange({fieldName.MakeMethod()}.Encode())"));
            }
         }

         memberMethod.Statements.Add(new CodeSnippetExpression("return result.ToArray()"));
         return memberMethod;
      }

      public static BuilderBase Init(string projectName, uint id, NodeTypeComposite typeDef, NodeTypeResolver typeDict)
      {
         return new StructBuilder(projectName, id, typeDef, typeDict);
      }

      public override TypeBuilderBase Create()
      {
         var typeDef = TypeDef as NodeTypeComposite;

         ClassName = $"{typeDef.Path[^1]}";

         ReferenzName = $"{NamespaceName}.{typeDef.Path[^1]}";

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
         nameMethod.Comments.Add(new CodeCommentStatement("<inheritdoc/>", true));
         targetClass.Members.Add(nameMethod);

         if (typeDef.TypeFields != null)
         {
            for (int i = 0; i < typeDef.TypeFields.Length; i++)
            {
               NodeTypeField typeField = typeDef.TypeFields[i];
               string fieldName = StructBuilder.GetFieldName(typeField, "value", typeDef.TypeFields.Length, i);

               NodeTypeResolved fullItem = GetFullItemPath(typeField.TypeId);

               CodeTypeMember autoProperty = CreateAutoProperty(fieldName, fullItem.ToString());

               // add comment to propertiy if exists
               autoProperty.Comments.AddRange(GetComments(typeField.Docs, null, fieldName));

               targetClass.Members.Add(autoProperty);
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
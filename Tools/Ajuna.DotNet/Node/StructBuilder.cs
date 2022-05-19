using Ajuna.DotNet.Extensions;
using Ajuna.DotNet.Node.Base;
using Ajuna.NetApi.Model.Meta;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ajuna.DotNet.Node
{
   public class StructBuilder : TypeBuilderBase
   {
      private StructBuilder(uint id, NodeTypeComposite typeDef, Dictionary<uint, (string, List<string>)> typeDict)
          : base(id, typeDef, typeDict)
      {
      }

      private CodeMemberField GetPropertyField(string name, string baseType)
      {
         CodeMemberField field = new()
         {
            Attributes = MemberAttributes.Private,
            Name = name.MakePrivateField(),
            Type = new CodeTypeReference($"{baseType}")
         };
         return field;
      }

      private CodeMemberProperty GetProperty(string name, CodeMemberField propertyField)
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
         var decodeMethod = SimpleMethod("Decode");
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

               string fieldName = GetFieldName(typeField, "value", typeFields.Length, i);
               var fullItem = GetFullItemPath(typeField.TypeId);

               decodeMethod.Statements.Add(new CodeSnippetExpression($"{fieldName.MakeMethod()} = new {fullItem.Item1}()"));
               decodeMethod.Statements.Add(new CodeSnippetExpression($"{fieldName.MakeMethod()}.Decode(byteArray, ref p)"));
            }
         }

         decodeMethod.Statements.Add(new CodeSnippetExpression("TypeSize = p - start"));
         return decodeMethod;
      }

      private CodeMemberMethod GetEncode(NodeTypeField[] typeFields)
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
               string fieldName = GetFieldName(typeField, "value", typeFields.Length, i);

               encodeMethod.Statements.Add(new CodeSnippetExpression($"result.AddRange({fieldName.MakeMethod()}.Encode())"));
            }
         }

         encodeMethod.Statements.Add(new CodeSnippetExpression("return result.ToArray()"));
         return encodeMethod;
      }

      public static BuilderBase Init(uint id, NodeTypeComposite typeDef, Dictionary<uint, (string, List<string>)> typeDict)
      {
         return new StructBuilder(id, typeDef, typeDict);
      }

      public override TypeBuilderBase Create()
      {
         var typeDef = TypeDef as NodeTypeComposite;

         #region CREATE

         ClassName = $"{typeDef.Path.Last()}";

         ReferenzName = $"{NameSpace}.{typeDef.Path.Last()}";

         CodeNamespace typeNamespace = new(NameSpace);
         TargetUnit.Namespaces.Add(typeNamespace);

         var targetClass = new CodeTypeDeclaration(ClassName)
         {
            IsClass = true,
            TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed
         };
         targetClass.BaseTypes.Add(new CodeTypeReference("BaseType"));

         // add comment to class if exists
         targetClass.Comments.AddRange(GetComments(typeDef.Docs, typeDef));

         typeNamespace.Types.Add(targetClass);

         var nameMethod = SimpleMethod("TypeName", "System.String", ClassName);
         targetClass.Members.Add(nameMethod);

         if (typeDef.TypeFields != null)
         {
            for (int i = 0; i < typeDef.TypeFields.Length; i++)
            {

               NodeTypeField typeField = typeDef.TypeFields[i];
               string fieldName = GetFieldName(typeField, "value", typeDef.TypeFields.Length, i);

               var fullItem = GetFullItemPath(typeField.TypeId);

               var field = GetPropertyField(fieldName, fullItem.Item1);

               // add comment to field if exists
               field.Comments.AddRange(GetComments(typeField.Docs, null, fieldName));

               targetClass.Members.Add(field);
               targetClass.Members.Add(GetProperty(fieldName, field));
            }
         }

         CodeMemberMethod encodeMethod = GetEncode(typeDef.TypeFields);
         targetClass.Members.Add(encodeMethod);

         CodeMemberMethod decodeMethod = GetDecode(typeDef.TypeFields);
         targetClass.Members.Add(decodeMethod);

         #endregion

         return this;
      }

      private string GetFieldName(NodeTypeField typeField, string alterName, int length, int index)
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

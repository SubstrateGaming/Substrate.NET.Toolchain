using Ajuna.DotNet.Service.Node.Base;
using Ajuna.NetApi.Model.Meta;
using Ajuna.NetApi.Model.Types;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Ajuna.DotNet.Service.Node
{
   public class EnumBuilder : TypeBuilderBase
   {
      private EnumBuilder(string projectName, uint id, NodeTypeVariant typeDef, NodeTypeResolver typeDict)
          : base(projectName, id, typeDef, typeDict)
      {
      }

      public static EnumBuilder Init(string projectName, uint id, NodeTypeVariant typeDef, NodeTypeResolver typeDict)
      {
         return new EnumBuilder(projectName, id, typeDef, typeDict);
      }

      public override TypeBuilderBase Create()
      {
         var typeDef = TypeDef as NodeTypeVariant;

         string enumName = $"{typeDef.Path.Last()}";

         ClassName = $"Enum{enumName}";
         
         ReferenzName = $"{NamespaceName}.{ClassName}";
         CodeNamespace typeNamespace = new(NamespaceName);
         TargetUnit.Namespaces.Add(typeNamespace);

         CodeTypeDeclaration TargetType = new(enumName)
         {
            IsEnum = true
         };

         if (typeDef.Variants != null)
         {
            foreach (string enumFieldName in typeDef.Variants.Select(p => p.Name))
            {
               TargetType.Members.Add(new CodeMemberField(ClassName, enumFieldName));
            }
         }
         typeNamespace.Types.Add(TargetType);

         if (typeDef.Variants != null)
         {
            var targetClass = new CodeTypeDeclaration(ClassName)
            {
               IsClass = true,
               TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed
            };
            targetClass.Comments.AddRange(GetComments(typeDef.Docs, typeDef));

            if (typeDef.Variants.All(p => p.TypeFields == null))
            {
               targetClass.BaseTypes.Add(new CodeTypeReference($"BaseEnum<{enumName}>"));
               typeNamespace.Types.Add(targetClass);
            }
            else
            {
               var codeTypeRef = new CodeTypeReference("BaseEnumExt");
               codeTypeRef.TypeArguments.Add(new CodeTypeReference(enumName));
               if (typeDef.Variants.Length < 29)
               {
                  for (int i = 0; i < typeDef.Variants.Length; i++)
                  {
                     TypeVariant variant = typeDef.Variants[i];
                     if (variant.TypeFields == null)
                     {
                        // add void type
                        codeTypeRef.TypeArguments.Add(new CodeTypeReference("BaseVoid"));
                     }
                     else
                     {
                        if (variant.TypeFields.Length == 1)
                        {
                           NodeTypeResolved item = GetFullItemPath(variant.TypeFields[0].TypeId);
                           codeTypeRef.TypeArguments.Add(new CodeTypeReference(item.ToString()));
                        }
                        else
                        {
                           var baseTuple = new CodeTypeReference("BaseTuple");

                           foreach (NodeTypeField field in variant.TypeFields)
                           {
                              NodeTypeResolved item = GetFullItemPath(field.TypeId);
                              baseTuple.TypeArguments.Add(new CodeTypeReference(item.ToString()));
                           }
                           codeTypeRef.TypeArguments.Add(baseTuple);
                        }
                     }
                  }
               }
               // Unhandled enumerations are manually done
               else
               {
                  codeTypeRef.TypeArguments.Add(new CodeTypeReference("BaseVoid"));

                  switch (enumName)
                  {
                     case "Era":
                        targetClass.Members.AddRange(GetEnumEra());
                        break;
                     case "Data":
                        targetClass.Members.AddRange(GetEnumData());
                        break;
                     // TODO (svnscha): Why is this not supported yet?
                     case "Event":
                     case "DispatchError":
                     case "Call":
                        break;
                     default:
                        throw new NotImplementedException("Enum extension can't handle such big sized typed rust enumeration, please create a manual fix for it.");
                  }
               }

               targetClass.BaseTypes.Add(codeTypeRef);
               typeNamespace.Types.Add(targetClass);
            }
         }
         return this;
      }

      private CodeTypeMemberCollection GetEnumEra()
      {

         ImportsNamespace.Imports.Add(new CodeNamespaceImport("Ajuna.NetApi.Model.Types"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport("Ajuna.NetApi.Model.Types.Primitive"));

         var result = new CodeTypeMemberCollection();

         CodeMemberMethod decodeMethod = SimpleMethod("Decode");
         decodeMethod.Parameters.Add(new()
         {
            Type = new CodeTypeReference("System.Byte[]"),
            Name = "byteArray"
         });
         decodeMethod.Parameters.Add(new()
         {
            Type = new CodeTypeReference("System.Int32"),
            Name = "p",
            Direction = FieldDirection.Ref
         });
         decodeMethod.Statements.Add(new CodeSnippetExpression("var start = p"));
         decodeMethod.Statements.Add(new CodeSnippetExpression("var enumByte = byteArray[p]"));
         decodeMethod.Statements.Add(new CodeSnippetExpression("Value = (Era)System.Enum.Parse(typeof(Era), enumByte.ToString(), true)"));
         decodeMethod.Statements.Add(new CodeSnippetExpression("p += 1"));
         decodeMethod.Statements.Add(new CodeSnippetExpression("Value2 = DecodeOneOf(enumByte, byteArray, ref p)"));
         decodeMethod.Statements.Add(new CodeSnippetExpression("TypeSize = p - start"));
         result.Add(decodeMethod);

         CodeMemberMethod decodeOneOfMethod = SimpleMethod("DecodeOneOf");
         decodeOneOfMethod.Attributes = MemberAttributes.Private;
         decodeOneOfMethod.ReturnType = new CodeTypeReference(typeof(IType));
         decodeOneOfMethod.Parameters.Add(new()
         {
            Type = new CodeTypeReference("System.Byte"),
            Name = "value"
         });
         decodeOneOfMethod.Parameters.Add(new()
         {
            Type = new CodeTypeReference("System.Byte[]"),
            Name = "byteArray"
         });
         decodeOneOfMethod.Parameters.Add(new()
         {
            Type = new CodeTypeReference("System.Int32"),
            Name = "p",
            Direction = FieldDirection.Ref
         });
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("IType result"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 0) { return null; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("result = new U8()"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("result.Decode(byteArray, ref p)"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("return result"));
         result.Add(decodeOneOfMethod);

         return result;
      }

      private CodeTypeMemberCollection GetEnumData()
      {
         ImportsNamespace.Imports.Add(new CodeNamespaceImport($"System"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport("Ajuna.NetApi.Model.Types"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport("Ajuna.NetApi.Model.Types.Primitive"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport($"{ProjectName}.Generated.Types.Base"));

         var result = new CodeTypeMemberCollection();

         CodeMemberMethod decodeMethod = SimpleMethod("Decode");
         decodeMethod.Parameters.Add(new()
         {
            Type = new CodeTypeReference("System.Byte[]"),
            Name = "byteArray"
         });
         decodeMethod.Parameters.Add(new()
         {
            Type = new CodeTypeReference("System.Int32"),
            Name = "p",
            Direction = FieldDirection.Ref
         });
         decodeMethod.Statements.Add(new CodeSnippetExpression("var start = p"));
         decodeMethod.Statements.Add(new CodeSnippetExpression("var enumByte = byteArray[p]"));
         decodeMethod.Statements.Add(new CodeSnippetExpression("Value = (Data)System.Enum.Parse(typeof(Data), enumByte.ToString(), true)"));
         decodeMethod.Statements.Add(new CodeSnippetExpression("p += 1"));
         decodeMethod.Statements.Add(new CodeSnippetExpression("Value2 = DecodeOneOf(enumByte, byteArray, ref p)"));
         decodeMethod.Statements.Add(new CodeSnippetExpression("TypeSize = p - start"));
         decodeMethod.Statements.Add(new CodeSnippetExpression("Bytes = new byte[TypeSize]"));
         decodeMethod.Statements.Add(new CodeSnippetExpression("Array.Copy(byteArray, start, base.Bytes, 0, TypeSize)"));

         result.Add(decodeMethod);

         CodeMemberMethod decodeOneOfMethod = SimpleMethod("DecodeOneOf");
         decodeOneOfMethod.Attributes = MemberAttributes.Private;
         decodeOneOfMethod.ReturnType = new CodeTypeReference(typeof(IType));
         decodeOneOfMethod.Parameters.Add(new()
         {
            Type = new CodeTypeReference("System.Byte"),
            Name = "value"
         });
         decodeOneOfMethod.Parameters.Add(new()
         {
            Type = new CodeTypeReference("System.Byte[]"),
            Name = "byteArray"
         });
         decodeOneOfMethod.Parameters.Add(new()
         {
            Type = new CodeTypeReference("System.Int32"),
            Name = "p",
            Direction = FieldDirection.Ref
         });
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("IType result"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 0) { return new BaseVoid(); }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 1) { result = new Arr0U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 2) { result = new Arr1U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 3) { result = new Arr2U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 4) { result = new Arr3U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 5) { result = new Arr4U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 6) { result = new Arr5U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 7) { result = new Arr6U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 8) { result = new Arr7U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 9) { result = new Arr8U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 10) { result = new Arr9U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 11) { result = new Arr10U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 12) { result = new Arr11U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 13) { result = new Arr12U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 15) { result = new Arr14U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 16) { result = new Arr15U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 17) { result = new Arr16U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 18) { result = new Arr17U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 19) { result = new Arr18U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 20) { result = new Arr19U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 21) { result = new Arr20U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 22) { result = new Arr21U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 23) { result = new Arr22U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 24) { result = new Arr23U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 25) { result = new Arr24U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 26) { result = new Arr25U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 27) { result = new Arr26U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 28) { result = new Arr27U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 29) { result = new Arr28U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 30) { result = new Arr29U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 31) { result = new Arr30U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 32) { result = new Arr31U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 33) { result = new Arr32U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 34) { result = new Arr32U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 35) { result = new Arr32U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 36) { result = new Arr32U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("if (value == 37) { result = new Arr32U8(); result.Decode(byteArray, ref p); return result; }"));
         decodeOneOfMethod.Statements.Add(new CodeSnippetExpression("throw new NotImplementedException(\"Invalid leading byte, please check source\");"));
         result.Add(decodeOneOfMethod);

         return result;
      }
   }
}

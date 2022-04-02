﻿using Ajuna.NetApi.Model.Meta;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RuntimeMetadata
{
    public class ArrayGenBuilder : TypeBuilder
    {
        public static int Counter = 0;
        private ArrayGenBuilder(uint id, NodeTypeArray typeDef, Dictionary<uint, (string, List<string>)> typeDict)
            : base(id, typeDef, typeDict)
        {
        }

        private CodeMemberMethod GetDecode(string baseType)
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
            decodeMethod.Statements.Add(new CodeSnippetExpression($"var array = new {baseType}[TypeSize]"));
            decodeMethod.Statements.Add(new CodeSnippetExpression("for (var i = 0; i < array.Length; i++) " +
                "{" +
                $"var t = new {baseType}();" +
                "t.Decode(byteArray, ref p);" +
                "array[i] = t;" +
                "}"));
            decodeMethod.Statements.Add(new CodeSnippetExpression("var bytesLength = p - start"));
            decodeMethod.Statements.Add(new CodeSnippetExpression("Bytes = new byte[bytesLength]"));
            decodeMethod.Statements.Add(new CodeSnippetExpression("Array.Copy(byteArray, start, Bytes, 0, bytesLength)"));
            decodeMethod.Statements.Add(new CodeSnippetExpression("Value = array"));
            return decodeMethod;
        }

        private CodeMemberMethod GetEncode()
        {
            CodeMemberMethod encodeMethod = new()
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Override,
                Name = "Encode",
                ReturnType = new CodeTypeReference("System.Byte[]")
            };
            encodeMethod.Statements.Add(new CodeSnippetExpression("var result = new List<byte>()"));
            encodeMethod.Statements.Add(new CodeSnippetExpression("foreach (var v in Value)" +
                "{" +
                "result.AddRange(v.Encode());" +
                "}"));
            encodeMethod.Statements.Add(new CodeSnippetExpression("return result.ToArray()"));
            return encodeMethod;
        }

        public static ArrayGenBuilder Create(uint id, NodeTypeArray nodeType, Dictionary<uint, (string, List<string>)> typeDict)
        {
            return new ArrayGenBuilder(id, nodeType, typeDict);
        }

        public override TypeBuilder Create()
        {
            var typeDef = TypeDef as NodeTypeArray;

            #region CREATE

            var fullItem = GetFullItemPath(typeDef.TypeId);

            ClassName = $"Arr{typeDef.Length}{fullItem.Item1.Split('.').Last()}";
            CodeNamespace typeNamespace = new(NameSpace);
            TargetUnit.Namespaces.Add(typeNamespace);

            if (ClassName.Any(ch => !Char.IsLetterOrDigit(ch)))
            {
                Counter++;
                ClassName = $"Arr{typeDef.Length}Special" + Counter++;
            }

            ReferenzName = $"{NameSpace}.{ClassName}";

            var targetClass = new CodeTypeDeclaration(ClassName)
            {
                IsClass = true,
                TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed
            };
            targetClass.BaseTypes.Add(new CodeTypeReference("BaseType"));

            // add comment to class if exists
            targetClass.Comments.AddRange(GetComments(typeDef.Docs, typeDef));

            typeNamespace.Types.Add(targetClass);

            // Declaring a name method
            CodeMemberMethod nameMethod = new()
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Override,
                Name = "TypeName",
                ReturnType = new CodeTypeReference(typeof(System.String))
            };
            var methodRef1 = new CodeMethodReferenceExpression(new CodeObjectCreateExpression(fullItem.Item1, Array.Empty<CodeExpression>()), "TypeName()");
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
                Type = new CodeTypeReference(typeof(System.Int32))
            };
            sizeProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression((int)typeDef.Length)));
            targetClass.Members.Add(sizeProperty);


            CodeMemberMethod encodeMethod = GetEncode();
            targetClass.Members.Add(encodeMethod);

            CodeMemberMethod decodeMethod = GetDecode(fullItem.Item1);
            targetClass.Members.Add(decodeMethod);


            CodeMemberField valueField = new()
            {
                Attributes = MemberAttributes.Private,
                Name = "_value",
                Type = new CodeTypeReference($"{fullItem.Item1}[]")
            };
            targetClass.Members.Add(valueField);
            CodeMemberProperty valueProperty = new()
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                Name = "Value",
                HasGet = true,
                HasSet = true,
                Type = new CodeTypeReference($"{fullItem.Item1}[]")
            };
            valueProperty.GetStatements.Add(new CodeMethodReturnStatement(
                new CodeFieldReferenceExpression(
                new CodeThisReferenceExpression(), valueField.Name)));
            valueProperty.SetStatements.Add(new CodeAssignStatement(
                new CodeFieldReferenceExpression(
                    new CodeThisReferenceExpression(), valueField.Name),
                                    new CodePropertySetValueReferenceExpression()));


            CodeMemberMethod createMethod = new()
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                Name = "Create"
            };
            createMethod.Parameters.Add(new()
            {
                Type = new CodeTypeReference($"{fullItem.Item1}[]"),
                Name = "array"
            });
            createMethod.Statements.Add(new CodeSnippetExpression("Value = array"));
            createMethod.Statements.Add(new CodeSnippetExpression("Bytes = Encode()"));
            targetClass.Members.Add(createMethod);

            targetClass.Members.Add(valueProperty);
            #endregion

            return this;
        }
    }
}

using Ajuna.DotNet.Client.Interfaces;
using Ajuna.NetApi;
using Ajuna.NetApi.Attributes;
using Ajuna.NetApi.Model.Types;
using Ajuna.NetApi.Model.Types.Base;
using Ajuna.NetApi.Model.Types.Metadata.V14;
using Ajuna.NetApi.Model.Types.Primitive;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Ajuna.DotNet.Extensions
{
   /// <summary>
   /// Simplifies access to ReflectedEndpoint interface.
   /// </summary>
   internal static class ReflectedEndpointExtensions
   {
      /// <summary>
      /// Converts a reflected endpoint to an interface method code element.
      /// The generated method is being implemented by an actual client.
      /// </summary>
      /// <param name="endpoint">The endpoint to generate the interface method for.</param>
      /// <param name="currentNamespace">The current namespace where the generated class will be attached to.</param>
      internal static CodeTypeMember ToInterfaceMethod(this IReflectedEndpoint endpoint, CodeNamespace currentNamespace)
      {
         var method = new CodeMemberMethod()
         {
            Name = endpoint.GetClientMethodName(),
            ReturnType = endpoint.GetResponse().ToInterfaceMethodReturnType(currentNamespace),
         };

         method.Parameters.AddRange(endpoint.GetRequest().ToInterfaceMethodParameters());
         return method;
      }

      /// <summary>
      /// Converts a reflected endpoint to an interface method code element.
      /// The generated method is being implemented by an actual client.
      /// </summary>
      /// <param name="endpoint">The endpoint to generate the interface method for.</param>
      /// <param name="currentNamespace">The current namespace where the generated class will be attached to.</param>
      internal static CodeTypeMember ToMockupInterfaceMethod(this IReflectedEndpoint endpoint, CodeNamespace currentNamespace)
      {
         var method = new CodeMemberMethod()
         {
            Name = endpoint.GetClientMethodName().Replace("Get", "Set"),
            ReturnType = new CodeTypeReference(typeof(Task<>).MakeGenericType(new[] { typeof(bool) }))
         };

         IReflectedEndpointType defaultReturnType = endpoint.GetResponse().GetSuccessReturnType();
         if (defaultReturnType != null)
         {
            // Ensure we are importing all model items.
            // Not actually required since we use fully qualified items but we want to get rid of that later.
            currentNamespace.Imports.Add(new CodeNamespaceImport(defaultReturnType.Type.Namespace));
            method.Parameters.Add(new CodeParameterDeclarationExpression(defaultReturnType.Type, "value"));
         }


         method.Parameters.AddRange(endpoint.GetRequest().ToInterfaceMethodParameters());
         return method;
      }

      /// <summary>
      /// Converts a reflected controller endpoint to a client class method that implements the previously generated interface method.
      /// </summary>
      /// <param name="endpoint"></param>
      /// <param name="controller">The owning controller.</param>
      /// <param name="clientNamespace"></param>
      internal static CodeTypeMember ToClientMethod(this IReflectedEndpoint endpoint, IReflectedController controller, CodeNamespace clientNamespace)
      {
         var method = new CodeMemberMethod()
         {
            Name = endpoint.GetClientMethodName(),
            // TODO (svnscha): Should use async modifier directly to avoid patching the code.
            ReturnType = endpoint.GetResponse().ToInterfaceMethodReturnType(clientNamespace),
            Attributes = MemberAttributes.Public,
         };

         IReflectedEndpointRequest request = endpoint.GetRequest();

         method.Parameters.AddRange(request.ToInterfaceMethodParameters());

         var invokeArgumentType = new CodeTypeReference(endpoint.GetResponse().GetSuccessReturnType().Type);
         string endpointUrl = $"{controller.GetEndpointUrl()}/{endpoint.Endpoint.ToLower()}";

         if (method.Parameters.Count == 0)
         {
            method.Statements.Add(
               new CodeMethodReturnStatement(
                  new CodeMethodInvokeExpression(
                     new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "SendRequestAsync", invokeArgumentType),
                     new CodeVariableReferenceExpression("_httpClient"),
                     new CodePrimitiveExpression(endpointUrl)
               ))
            );
         }
         else
         {

            if (method.Parameters.Count == 1 && request.KeyBuilderAttribute.ParameterType != typeof(void))
            {
               method.Statements.Add(
                  new CodeMethodReturnStatement(
                     new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "SendRequestAsync", invokeArgumentType),
                        new CodeVariableReferenceExpression("_httpClient"),
                        new CodePrimitiveExpression(endpointUrl),
                        new CodeMethodInvokeExpression(
                           new CodeMethodReferenceExpression(
                              new CodeTypeReferenceExpression(request.KeyBuilderAttribute.ClassType),
                              request.KeyBuilderAttribute.MethodName),
                           new CodeVariableReferenceExpression(method.Parameters[0].Name)
                        )
                  ))
               );
            }
            else
            {
               method.Statements.Add(
                  new CodeMethodReturnStatement(
                     new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "SendRequestAsync", invokeArgumentType),
                        new CodeVariableReferenceExpression("_httpClient"),
                        new CodePrimitiveExpression(endpointUrl),
                        new CodeSnippetExpression(GetEncodeCallParameterList(method.Parameters))
                  ))
               );
            }

         }
         return method;
      }

      /// <summary>
      /// Converts a reflected controller endpoint to a client class method that implements the previously generated interface method.
      /// </summary>
      /// <param name="endpoint"></param>
      /// <param name="controller">The owning controller.</param>
      /// <param name="clientNamespace"></param>
      internal static CodeTypeMember ToMockupClientMethod(this IReflectedEndpoint endpoint, IReflectedController controller, CodeNamespace clientNamespace)
      {
         var method = new CodeMemberMethod()
         {
            Name = endpoint.GetClientMethodName().Replace("Get", "Set"),
            // TODO (svnscha): Should use async modifier directly to avoid patching the code.
            ReturnType = new CodeTypeReference(typeof(Task<>).MakeGenericType(new[] { typeof(bool) })),
            Attributes = MemberAttributes.Public,
         };

         IReflectedEndpointRequest request = endpoint.GetRequest();

         IReflectedEndpointType defaultReturnType = endpoint.GetResponse().GetSuccessReturnType();
         if (defaultReturnType != null)
         {
            // Ensure we are importing all model items.
            // Not actually required since we use fully qualified items but we want to get rid of that later.
            clientNamespace.Imports.Add(new CodeNamespaceImport(defaultReturnType.Type.Namespace));
            method.Parameters.Add(new CodeParameterDeclarationExpression(defaultReturnType.Type, "value"));
         }

         method.Parameters.AddRange(request.ToInterfaceMethodParameters());

         string endpointUrl = $"{controller.Name.Replace("Controller", string.Empty)}/{endpoint.Endpoint}";

         CodeExpression valueReference = new CodeSnippetExpression("value.Encode()");

         if (method.Parameters.Count == 1)
         {
            // Params() without key argument
            method.Statements.Add(
               new CodeMethodReturnStatement(
                  new CodeMethodInvokeExpression(
                     new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "SendMockupRequestAsync"),
                     new CodeVariableReferenceExpression("_httpClient"),
                     new CodePrimitiveExpression(endpointUrl),
                     valueReference,
                     new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(
                           new CodeTypeReferenceExpression(request.KeyBuilderAttribute.ClassType),
                           request.KeyBuilderAttribute.MethodName))
               ))
            );
         }
         else
         {
            // Params() with key argument
            method.Statements.Add(
               new CodeMethodReturnStatement(
                  new CodeMethodInvokeExpression(
                     new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "SendMockupRequestAsync"),
                     new CodeVariableReferenceExpression("_httpClient"),
                     new CodePrimitiveExpression(endpointUrl),
                     valueReference,
                     new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(
                           new CodeTypeReferenceExpression(request.KeyBuilderAttribute.ClassType),
                           request.KeyBuilderAttribute.MethodName),
                        new CodeVariableReferenceExpression("key"))
               ))
            );
         }



         return method;
      }

      /// <summary>
      /// Converts a reflected controller endpoint to a unit test client call method that implements the previously generated clients.
      /// </summary>
      /// <param name="endpoint"></param>
      /// <param name="controller">The owning controller.</param>
      /// <param name="clientNamespace"></param>
      internal static CodeTypeMember ToUnitTestMethod(this IReflectedEndpoint endpoint, CodeTypeMemberCollection currentMembers, IReflectedController controller, CodeNamespace clientNamespace)
      {
         var method = new CodeMemberMethod()
         {
            Name = endpoint.GetClientMethodName().Replace("Get", "Test"),
            // TODO (svnscha): Should use async modifier directly to avoid patching the code.
            ReturnType = new CodeTypeReference(typeof(Task)),
            Attributes = MemberAttributes.Public,
         };

         method.CustomAttributes.Add(new CodeAttributeDeclaration("Test"));

         clientNamespace.Imports.Add(new CodeNamespaceImport($"{clientNamespace.Name.Replace("Test", "Mockup")}.Clients"));
         clientNamespace.Imports.Add(new CodeNamespaceImport($"{clientNamespace.Name.Replace("Test.", "")}.Clients"));

         // var mockupClient = new MockupClient()
         GenerateNewMockupClientStatement(controller, method);

         IReflectedEndpointType defaultReturnType = endpoint.GetResponse().GetSuccessReturnType();
         if (defaultReturnType != null)
         {
            // Ensure we are importing all model items.
            // Not actually required since we use fully qualified items but we want to get rid of that later.
            clientNamespace.Imports.Add(new CodeNamespaceImport(defaultReturnType.Type.Namespace));

            //string[] expectedBaseTypeNames = new string[]
            //{
            //   "BaseEnum",
            //};

            //string[] expectedTypeNames = new string[]
            //{
            //   "BaseVec",
            //   "BaseTuple"
            //};

            //bool needCustomInitializerFunction = false;

            //PropertyInfo[] usableFields = GetUsableFields(defaultReturnType.Type);

            //if (usableFields.Length == 0)
            //{
            //   // We can directly initialize the given type (most likely).
            //   needCustomInitializerFunction = true;

            //   if (!IsPrimitiveType(defaultReturnType.Type)
            //      && !expectedTypeNames.Any(x => defaultReturnType.Type.Name.Contains(x))
            //      && !expectedBaseTypeNames.Any(x => defaultReturnType.Type.BaseType.Name.Contains(x)))
            //   {
            //      method.Statements.Add(new CodeSnippetStatement());
            //      method.Statements.Add(new CodeCommentStatement($"TODO: The type {defaultReturnType.Type.Name} cannot be initialized with testing values from code generator."));
            //      method.Statements.Add(new CodeCommentStatement("Please test this manually."));
            //      method.Statements.Add(new CodeSnippetStatement());

            //      // Not supported. Make sure to default initialize the type (even empty).
            //      needCustomInitializerFunction = false;
            //   }
            //}

            GenerateMockupValueStatement(currentMembers, method, defaultReturnType.Type);
         }
         else
         {
            throw new NotImplementedException("Setting mockup data without value does not work.");
         }

         // TODO (svnscha): Confirm this is always 1.
         CodeParameterDeclarationExpressionCollection methodParams = endpoint.GetRequest().ToInterfaceMethodParameters();

         if (methodParams.Count == 1)
         {
            IReflectedEndpointRequest request = endpoint.GetRequest();

            Type underlyingMethodType = request.GetInterfaceMethodParameterType();

            // var mockupKey = new ()
            GenerateMockupKeyStatement(currentMembers, method, underlyingMethodType);

            // Empty line
            method.Statements.Add(new CodeSnippetStatement());

            // bool mockupSetResult = await mockupClient. XXXXX (mockupValue, mockupKey);
            method.Statements.Add(new CodeCommentStatement($"Save the previously generated mockup value in RPC service storage."));
            method.Statements.Add(
               new CodeVariableDeclarationStatement(
                  new CodeTypeReference(typeof(bool)),
                  "mockupSetResult",
                  new CodeMethodInvokeExpression(
                     new CodeMethodReferenceExpression(
                        new CodeVariableReferenceExpression("mockupClient"), endpoint.GetClientMethodName().Replace("Get", "Set")),
                     new CodeVariableReferenceExpression("mockupValue"),
                     new CodeVariableReferenceExpression("mockupKey")
                  )
               ));
         }
         else
         {
            // Empty line
            method.Statements.Add(new CodeSnippetStatement());

            // bool mockupSetResult = await mockupClient. XXXXX (mockupValue);
            method.Statements.Add(new CodeCommentStatement($"Save the previously generated mockup value in RPC service storage."));
            method.Statements.Add(
               new CodeVariableDeclarationStatement(
                  new CodeTypeReference(typeof(bool)),
                  "mockupSetResult",
                  new CodeMethodInvokeExpression(
                     new CodeMethodReferenceExpression(
                        new CodeVariableReferenceExpression("mockupClient"), endpoint.GetClientMethodName().Replace("Get", "Set")),
                     new CodeVariableReferenceExpression("mockupValue")
               )));
         }

         // Assert.IsTrue(mockupSetResult)
         GenerateAssertIsTrueStatement(method);

         // Empty line
         method.Statements.Add(new CodeSnippetStatement());

         // var mockupClient = new Client()
         method.Statements.Add(new CodeCommentStatement($"Construct new RPC client to test against."));
         method.Statements.Add(
            new CodeVariableDeclarationStatement(
               new CodeTypeReference(controller.GetClientClassName()),
               "rpcClient",
               new CodeSnippetExpression($"new {controller.GetClientClassName()}(_httpClient)")));

         // bool rpcResult = await mockupClient.GetXXXXX (mockupKey);
         if (methodParams.Count == 1)
         {
            method.Statements.Add(
               new CodeVariableDeclarationStatement(
                  new CodeTypeReference(defaultReturnType.Type),
                  "rpcResult",
                  new CodeMethodInvokeExpression(
                     new CodeMethodReferenceExpression(
                        new CodeVariableReferenceExpression("rpcClient"), endpoint.GetClientMethodName()),
                     new CodeVariableReferenceExpression("mockupKey")
               )));
         }
         else
         {
            method.Statements.Add(
               new CodeVariableDeclarationStatement(
                  new CodeTypeReference(defaultReturnType.Type),
                  "rpcResult",
                  new CodeMethodInvokeExpression(
                     new CodeMethodReferenceExpression(
                        new CodeVariableReferenceExpression("rpcClient"), endpoint.GetClientMethodName())
               )));
         }


         // Assert.AreEqual(mockupValue.Encode(), rpcResult.Encode());
         method.Statements.Add(new CodeSnippetStatement());
         method.Statements.Add(new CodeCommentStatement("Test that the expected mockup value matches the actual result from RPC service."));
         method.Statements.Add(new CodeSnippetExpression("Assert.AreEqual(mockupValue.Encode(), rpcResult.Encode())"));

         return method;
      }

      private static void GenerateAssertIsTrueStatement(CodeMemberMethod method)
      {
         method.Statements.Add(new CodeSnippetStatement());
         method.Statements.Add(new CodeCommentStatement("Test that the expected mockup value was handled successfully from RPC service."));
         method.Statements.Add(new CodeSnippetExpression("Assert.IsTrue(mockupSetResult)"));
      }

      private static void GenerateMockupKeyStatement(CodeTypeMemberCollection currentMembers, CodeMemberMethod method, Type underlyingMethodType)
      {
         method.Statements.Add(new CodeVariableDeclarationStatement(underlyingMethodType, "mockupKey", new CodeMethodInvokeExpression(CallGetTestValue(currentMembers, underlyingMethodType))));
      }

      private static void GenerateMockupValueStatement(CodeTypeMemberCollection currentMembers, CodeMemberMethod method, Type elementType)
      {
         method.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(elementType), "mockupValue", new CodeMethodInvokeExpression(CallGetTestValue(currentMembers, elementType))));
      }

      private static void GenerateNewMockupClientStatement(IReflectedController controller, CodeMemberMethod method)
      {
         method.Statements.Add(new CodeCommentStatement($"Construct new Mockup client to test with."));
         method.Statements.Add(
            new CodeVariableDeclarationStatement(
               new CodeTypeReference(controller.GetMockupClientClassName()),
               "mockupClient",
               new CodeSnippetExpression($"new {controller.GetMockupClientClassName()}(_httpClient)")));
      }

      //private static void InitializeUsableFields(string variableReference, CodeTypeMemberCollection currentMembers, CodeMemberMethod method, PropertyInfo[] usableFields)
      //{
      //   foreach (PropertyInfo field in usableFields)
      //   {
      //      method.Statements.Add(new CodeAssignStatement(
      //         new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(variableReference), field.Name),
      //         new CodeMethodInvokeExpression(
      //            CallGetTestValue(currentMembers, field.PropertyType))
      //      ));
      //   }
      //}

      private static PropertyInfo[] GetUsableFields(Type type)
      {
         IEnumerable<PropertyInfo> fields = type.GetTypeInfo().DeclaredProperties;
         return fields.Where(field => field.CanWrite && field.SetMethod.IsPublic && field.Name != "TypeSize").ToArray();
      }

      /// <summary>
      /// Returns a client method name for the given endpoint.
      /// </summary>
      /// <param name="endpoint">The endpoint to query the client method name for.</param>
      internal static string GetClientMethodName(this IReflectedEndpoint endpoint) => endpoint.Name;

      private static bool IsArrayInitializerField(Type type)
      {
         if (type.Name == "BaseVec`1")
         {
            return true;
         }

         AjunaNodeTypeAttribute attribute = type.GetCustomAttribute<AjunaNodeTypeAttribute>(false);
         if (attribute == null)
         {
            return false;
         }

         return attribute.NodeType == TypeDefEnum.Array;
      }

      private static bool IsBaseTupleField(Type type)
      {
         return type.Name.StartsWith("BaseTuple");
      }

      private static bool IsBaseOptField(Type type)
      {
         return type.Name.StartsWith("BaseOpt");
      }

      private static bool IsBaseComField(Type type)
      {
         return type.Name.StartsWith("BaseCom");
      }

      private static bool IsBaseEnumType(Type type)
      {
         if (type.BaseType?.Name == "BaseEnum`1")
         {
            return true;
         }

         return false;
      }

      private static bool IsBaseEnumExtType(Type type)
      {
         if (type.BaseType?.Name.StartsWith("BaseEnumExt`") ?? false)
         {
            return true;
         }

         return false;
      }

      private static bool IsPrimitiveSystemTypeBoolean(Type type) => type == typeof(bool);

      private static bool IsPrimitiveType(Type type)
      {
         var expectedPrimitiveTypes = new Type[]
         {
            typeof(BaseVoid),
            typeof(Bool),
            typeof(I8),
            typeof(I16),
            typeof(I32),
            typeof(I64),
            typeof(I128),
            typeof(I256),
            typeof(U8),
            typeof(U16),
            typeof(U32),
            typeof(U64),
            typeof(U128),
            typeof(U256),
            typeof(PrimChar),
            typeof(Str),
         };

         return expectedPrimitiveTypes.Contains(type);
      }

      private static CodeMethodReferenceExpression CallGetTestValue(CodeTypeMemberCollection currentMembers, Type type)
      {
         if (IsPrimitiveType(type))
         {
            return new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), $"GetTestValue{type.Name}");
         }

         //// Build a wrapper function for the type
         //// TODO (svnscha): Remove again.
         //string functionName = $"GetTestValue{type.Name}";

         //if (functionName == "GetTestValueBalanceLock")
         //{
         //   Debugger.Break();
         //}

         //bool found = false;

         //if (type.IsGenericType)
         //{
         //   // Generate a unique suffix to avoid name collisions.
         //   int i = 0;

         //   do
         //   {
         //      i++;
         //      functionName = $"GetTestValueGeneric{i}";

         //   } while (HasFunctionDeclared(currentMembers, functionName));
         //}
         //else
         //{
         //   found = HasFunctionDeclared(currentMembers, functionName);
         //}

         //if (!found)
         //{
         //   var method = new CodeMemberMethod()
         //   {
         //      Name = functionName,
         //      ReturnType = new CodeTypeReference(type),
         //      Attributes = MemberAttributes.Public | MemberAttributes.Final,
         //   };

         //   currentMembers.Add(method);

         //   method.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(type), "result"));

         //   if (IsPrimitiveType(type))
         //   {
         //      method.Statements.Add(new CodeAssignStatement(
         //         new CodeVariableReferenceExpression("result"),
         //         new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "GetTestValue", new CodeTypeReference(type)))
         //      ));
         //   }
         //   else 
         //   {
         //      method.Statements.Add(new CodeAssignStatement(
         //         new CodeVariableReferenceExpression("result"),
         //         new CodeObjectCreateExpression(new CodeTypeReference(type))
         //      ));
         //   }

         //   if (IsInitializerField(type.Name))
         //   {
         //      // Can be directly initialized.
         //      GenerateFieldInitializer(currentMembers, new CodeVariableReferenceExpression("result"), method, type);
         //   }
         //   else
         //   {
         //      // Requires property initialization.
         //      PropertyInfo[] usableFields = GetUsableFields(type);
         //      if (usableFields.Length > 0)
         //      {
         //         // Now we must initialize the fields in this complex type.
         //         foreach (PropertyInfo field in usableFields)
         //         {
         //            method.Statements.Add(new CodeAssignStatement(
         //               new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("result"), field.Name),
         //               new CodeObjectCreateExpression(new CodeTypeReference(field.PropertyType)))
         //            );

         //            Type propType = field.PropertyType;
         //            CodeExpression targetObject = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("result"), field.Name);
         //            GenerateFieldInitializer(currentMembers, targetObject, method, propType);
         //         }

         //      }
         //   }

         //   method.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("result")));
         //}

         var method = new CodeMemberMethod()
         {
            Name = $"GetTestValue{currentMembers.Count}",
            ReturnType = new CodeTypeReference(type),
            Attributes = MemberAttributes.Public | MemberAttributes.Final,
         };

         currentMembers.Add(method);

         method.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(type), "result"));

         var targetObject = new CodeVariableReferenceExpression("result");

         // Initialize properties.
         PropertyInfo[] usableFields = GetUsableFields(type);

         if (!GenerateDirectInitializeStatement(currentMembers, method, type, targetObject))
         {
            if (usableFields.Length > 0)
            {
               foreach (PropertyInfo usableField in usableFields)
               {
                  Type fieldType = usableField.PropertyType;
                  var fieldReference = new CodeFieldReferenceExpression(targetObject, usableField.Name);

                  if (!GenerateDirectInitializeStatement(currentMembers, method, fieldType, fieldReference))
                  {
                     method.Statements.Add(new CodeAssignStatement(
                        fieldReference,
                        new CodeMethodInvokeExpression(CallGetTestValue(currentMembers, fieldType))));
                  }
               }
            }
            else
            {
               method.Statements.Add(new CodeCommentStatement($"NOT IMPLEMENTED >> Initialize {type.FullName}"));
            }
         }
         method.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("result")));

         return new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), method.Name);
      }

      private static bool GenerateDirectInitializeStatement(CodeTypeMemberCollection currentMembers, CodeMemberMethod method, Type fieldType, CodeExpression fieldReference)
      {
         if (IsArrayInitializerField(fieldType))
         {
            method.Statements.Add(new CodeAssignStatement(fieldReference, new CodeObjectCreateExpression(fieldType)));
            GenerateArrayInitializeStatement(currentMembers, method, fieldType, fieldReference);
            return true;
         }
         else if (IsBaseEnumType(fieldType))
         {
            method.Statements.Add(new CodeAssignStatement(fieldReference, new CodeObjectCreateExpression(fieldType)));
            GenerateBaseEnumInitializeStatement(method, fieldType.BaseType, fieldReference);
            return true;
         }
         else if (IsBaseEnumExtType(fieldType))
         {
            method.Statements.Add(new CodeAssignStatement(fieldReference, new CodeObjectCreateExpression(fieldType)));
            GenerateBaseEnumExtInitializeStatement(currentMembers, method, fieldType.BaseType, fieldReference);
            return true;
         }
         else if (IsBaseTupleField(fieldType) || IsBaseOptField(fieldType) || IsBaseComField(fieldType))
         {
            method.Statements.Add(new CodeAssignStatement(fieldReference, new CodeObjectCreateExpression(fieldType)));

            if (fieldType.IsGenericType)
            {
               GenerateGenericTypeArgumentInitializeStatement(currentMembers, method, fieldType, fieldReference);
            }

            return true;
         }
         else if (IsPrimitiveType(fieldType))
         {
            method.Statements.Add(new CodeAssignStatement(
               fieldReference,
               new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), $"GetTestValue{fieldType.Name}")));

            return true;
         }
         else
         {
            method.Statements.Add(new CodeAssignStatement(fieldReference, new CodeObjectCreateExpression(fieldType)));
         }

         return false;
      }

      private static void GenerateBaseEnumInitializeStatement(CodeMemberMethod method, Type propertyType, CodeExpression fieldReference)
      {
         Type elementType = propertyType.GetGenericArguments()[0];
         method.Statements.Add(new CodeMethodInvokeExpression(fieldReference, "Create",
            new CodeMethodInvokeExpression(
               new CodeMethodReferenceExpression(
                  new CodeThisReferenceExpression(), "GetTestValueEnum", new CodeTypeReference(elementType)))));
      }

      private static void GenerateBaseEnumExtInitializeStatement(CodeTypeMemberCollection currentMembers, CodeMemberMethod method, Type propertyType, CodeExpression fieldReference)
      {
         //
         // Each enum value may have a different value type.
         // Since we use the first enum value in GetTestValueEnum() call we can simply use the first value type argument.
         //
         Type elementType = propertyType.GetGenericArguments()[0];
         Type valueType = propertyType.GetGenericArguments()[1];

         method.Statements.Add(new CodeMethodInvokeExpression(fieldReference, "Create",
            new CodeMethodInvokeExpression(
               new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "GetTestValueEnum", new CodeTypeReference(elementType))),
               new CodeMethodInvokeExpression(CallGetTestValue(currentMembers, valueType))
            ));
      }

      private static void GenerateGenericTypeArgumentInitializeStatement(CodeTypeMemberCollection currentMembers, CodeMemberMethod method, Type propertyType, CodeExpression fieldReference)
      {
         Type[] arguments = propertyType.GenericTypeArguments.ToArray();

         var arrayInitialize = new CodeExpression[arguments.Length];
         for (int i = 0; i < arguments.Length; i++)
         {
            if (IsBaseComField(propertyType))
            {
               arrayInitialize[i] =
                  new CodeObjectCreateExpression(typeof(CompactInteger),
                     new CodeFieldReferenceExpression(
                        new CodeMethodInvokeExpression(CallGetTestValue(currentMembers, arguments[i])),
                        "Value"));
            }
            else
            {
               arrayInitialize[i] = new CodeMethodInvokeExpression(CallGetTestValue(currentMembers, arguments[i]));
            }
         }

         // Get the actual BaseTuple IType type field.
         FieldInfo privateValueFieldInfo = propertyType.GetTypeInfo().DeclaredFields.FirstOrDefault(x => x.FieldType.IsArray);
         Type elementType = privateValueFieldInfo?.FieldType.GetElementType() ?? null;
         method.Statements.Add(new CodeMethodInvokeExpression(fieldReference, "Create", arrayInitialize));
      }

      private static void GenerateArrayInitializeStatement(CodeTypeMemberCollection currentMembers, CodeMemberMethod method, Type propertyType, CodeExpression fieldReference)
      {
         Type elementType = null;

         int elementArraySize = 0;

         if (propertyType.IsGenericType)
         {
            // Vectors should be of "any" size. So we simply pass one.
            elementType = propertyType.GetGenericArguments()[0];
            elementArraySize = 1;
         }
         else
         {
            // Get the actual array type field.
            var instanced = Activator.CreateInstance(propertyType.GetTypeInfo()) as IType;

            FieldInfo privateValueFieldInfo = propertyType.GetTypeInfo().DeclaredFields.FirstOrDefault(x => x.Name == "_value");
            elementType = privateValueFieldInfo?.FieldType.GetElementType() ?? null;
            elementArraySize = instanced.TypeSize;
         }

         if (elementType == null)
         {
            method.Statements.Add(new CodeThrowExceptionStatement(new CodeObjectCreateExpression(typeof(InvalidOperationException), new CodePrimitiveExpression("Generator could not deduct array initializer element type!"))));
         }
         else
         {
            var arrayInitialize = new CodeExpression[elementArraySize];
            for (int i = 0; i < elementArraySize; i++)
            {
               arrayInitialize[i] = new CodeMethodInvokeExpression(CallGetTestValue(currentMembers, elementType));
            }

            method.Statements.Add(new CodeMethodInvokeExpression(fieldReference, "Create", new CodeArrayCreateExpression(elementType, arrayInitialize)));
         }
      }

      private static bool HasFunctionDeclared(CodeTypeMemberCollection currentMembers, string functionName)
      {
         bool found = false;
         foreach (CodeTypeMember member in currentMembers)
         {
            if (member.Name == functionName)
            {
               found = true;
               break;
            }
         }

         return found;
      }

      // Utility to build a parameter list separated by comma.
      private static string GetEncodeCallParameterList(CodeParameterDeclarationExpressionCollection parameters)
      {
         var parameterList = new List<string>();
         foreach (CodeParameterDeclarationExpression parameter in parameters)
         {
            // parameterList.Add($"{parameter.Name}{nameSuffix}");
            parameterList.Add(parameter.Name);
         }

         return string.Join(", ", parameterList);
      }
   }
}

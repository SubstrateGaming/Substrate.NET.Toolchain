using Ajuna.DotNet.Client.Interfaces;
using Ajuna.NetApi.Model.Types;
using Ajuna.NetApi.Model.Types.Base;
using Ajuna.NetApi.Model.Types.Primitive;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
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
                        new CodeSnippetExpression(GetEncodeCallParameterList(method.Parameters, string.Empty))
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

         string endpointUrl = $"{controller.GetEndpointUrl()}/{endpoint.Endpoint.ToLower()}";

         method.Statements.Add(
            new CodeMethodReturnStatement(
               new CodeMethodInvokeExpression(
                  new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "SendMockupRequestAsync"),
                  new CodeVariableReferenceExpression("_httpClient"),
                  new CodePrimitiveExpression(endpointUrl),
                  new CodeSnippetExpression(GetEncodeCallParameterList(method.Parameters, ".Encode()"))
            ))
         );

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
            ReturnType = new CodeTypeReference(typeof(void)),
            Attributes = MemberAttributes.Public,
         };

         method.CustomAttributes.Add(new CodeAttributeDeclaration("Test"));

         clientNamespace.Imports.Add(new CodeNamespaceImport($"{clientNamespace.Name.Replace("Test", "Mockup")}.Clients"));
         clientNamespace.Imports.Add(new CodeNamespaceImport($"{clientNamespace.Name.Replace("Test.", "")}.Clients"));

         // var mockupClient = new MockupClient()
         method.Statements.Add(new CodeCommentStatement($"Construct new Mockup client to test with."));
         method.Statements.Add(
            new CodeVariableDeclarationStatement(
               new CodeTypeReference(controller.GetMockupClientClassName()),
               "mockupClient",
               new CodeSnippetExpression($"new {controller.GetMockupClientClassName()}(_httpClient)")));

         IReflectedEndpointType defaultReturnType = endpoint.GetResponse().GetSuccessReturnType();
         if (defaultReturnType != null)
         {
            // Ensure we are importing all model items.
            // Not actually required since we use fully qualified items but we want to get rid of that later.
            clientNamespace.Imports.Add(new CodeNamespaceImport(defaultReturnType.Type.Namespace));

            string[] expectedBaseTypeNames = new string[]
            {
               "BaseEnum",
            };

            string[] expectedTypeNames = new string[]
            {
               "BaseVec",
               "BaseTuple"
            };

            bool needCustomInitializerFunction = false;

            IEnumerable<PropertyInfo> fields = defaultReturnType.Type.GetTypeInfo().DeclaredProperties;
            PropertyInfo[] usableFields = fields.Where(field => field.CanWrite && field.SetMethod.IsPublic && field.Name != "TypeSize").ToArray();

            if (usableFields.Length == 0)
            {
               // We can directly initialize the given type (most likely).
               needCustomInitializerFunction = true;

               if (!IsPrimitiveType(defaultReturnType.Type)
                  && !expectedTypeNames.Any(x => defaultReturnType.Type.Name.Contains(x))
                  && !expectedBaseTypeNames.Any(x => defaultReturnType.Type.BaseType.Name.Contains(x)))
               {
                  method.Statements.Add(new CodeSnippetStatement());
                  method.Statements.Add(new CodeCommentStatement($"TODO: The type {defaultReturnType.Type.Name} cannot be initialized with testing values from code generator."));
                  method.Statements.Add(new CodeCommentStatement("Please test this manually."));
                  method.Statements.Add(new CodeSnippetStatement());

                  // Not supported. Make sure to default initialize the type (even empty).
                  needCustomInitializerFunction = false;
               }
            }

            if (needCustomInitializerFunction)
            {
               // var mockupValue = new()
               method.Statements.Add(
                  new CodeVariableDeclarationStatement(
                     new CodeTypeReference(defaultReturnType.Type),
                     "mockupValue",
                     new CodeMethodInvokeExpression(
                        CallGetTestValue(currentMembers, defaultReturnType.Type))));
            }
            else
            {
               // var mockupValue = new()
               method.Statements.Add(
                  new CodeVariableDeclarationStatement(
                     new CodeTypeReference(defaultReturnType.Type),
                     "mockupValue",
                     new CodeObjectCreateExpression(defaultReturnType.Type)));

               foreach (PropertyInfo field in usableFields)
               {
                  method.Statements.Add(new CodeAssignStatement(
                     new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("mockupValue"), field.Name),
                     new CodeMethodInvokeExpression(
                        CallGetTestValue(currentMembers, field.PropertyType))
                  ));
               }
            }
         }
         else
         {
            throw new NotImplementedException("Setting mockup data without value does not work.");
         }

         // TODO (svnscha): Confirm this is always 1.
         CodeParameterDeclarationExpressionCollection methodParams = endpoint.GetRequest().ToInterfaceMethodParameters();

         if (methodParams.Count == 1)
         {
            // var mockupKey = new ()
            method.Statements.Add(
               new CodeVariableDeclarationStatement(
                  methodParams[0].Type,
                  "mockupKey",
                  new CodeObjectCreateExpression(methodParams[0].Type)));

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
               )));
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
         method.Statements.Add(new CodeSnippetStatement());
         method.Statements.Add(new CodeCommentStatement("Test that the expected mockup value was handled successfully from RPC service."));
         method.Statements.Add(new CodeSnippetExpression("Assert.IsTrue(mockupSetResult)"));

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

      /// <summary>
      /// Returns a client method name for the given endpoint.
      /// </summary>
      /// <param name="endpoint">The endpoint to query the client method name for.</param>
      internal static string GetClientMethodName(this IReflectedEndpoint endpoint) => endpoint.Name;

      private static bool IsPrimitiveType(Type type)
      {
         var expectedPrimitiveTypes = new Type[]
         {
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

         if (type.IsGenericType)
         {
            return new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "GetTestValue", new CodeTypeReference(type));
         }

         // Build a wrapper function for the type
         string functionName = $"GetTestValue{type.Name}";
         
         bool found = false;
         foreach (CodeTypeMember member in currentMembers)
         {
            if (member.Name == functionName)
            {
               found = true;
               break;
            }
         }

         if (!found)
         {
            var method = new CodeMemberMethod()
            {
               Name = functionName,
               ReturnType = new CodeTypeReference(type),
               Attributes = MemberAttributes.Public | MemberAttributes.Final,
            };

            method.Statements.Add(
               new CodeMethodReturnStatement(
                  new CodeMethodInvokeExpression(
                     new CodeMethodReferenceExpression(
                        new CodeThisReferenceExpression(), "GetTestValue", new CodeTypeReference(type)))));

            currentMembers.Add(method);
         }

         return new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), functionName);
      }

      // Utility to build a parameter list separated by comma.
      private static string GetEncodeCallParameterList(CodeParameterDeclarationExpressionCollection parameters, string nameSuffix)
      {
         var parameterList = new List<string>();
         foreach (CodeParameterDeclarationExpression parameter in parameters)
         {
            parameterList.Add($"{parameter.Name}{nameSuffix}");
         }

         return string.Join(", ", parameterList);
      }
   }
}

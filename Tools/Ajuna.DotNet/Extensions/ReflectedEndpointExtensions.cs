using Ajuna.DotNet.Client.Interfaces;
using System.CodeDom;
using System.Collections.Generic;
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
      /// Returns a client method name for the given endpoint.
      /// </summary>
      /// <param name="endpoint">The endpoint to query the client method name for.</param>
      internal static string GetClientMethodName(this IReflectedEndpoint endpoint) => endpoint.Name;

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

using System.CodeDom;
using System.Collections.Generic;
using Ajuna.RestService.ClientGenerator.Interfaces;

namespace Ajuna.RestService.ClientGenerator.Extensions
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

         method.Parameters.AddRange(endpoint.GetRequest().ToInterfaceMethodParameters(currentNamespace));
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
            // TODO: Find out how to use 'async' modifier. Internet says it doesn't work yet? Is this true?
            ReturnType = endpoint.GetResponse().ToInterfaceMethodReturnType(clientNamespace),
            Attributes = MemberAttributes.Public,
         };

         method.Parameters.AddRange(endpoint.GetRequest().ToInterfaceMethodParameters(clientNamespace));

         var invokeArgumentType = new CodeTypeReference(endpoint.GetResponse().GetSuccessReturnType().Type);
         var endpointUrl = $"{controller.GetEndpointUrl()}/{endpoint.Endpoint.ToLower()}";

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
         return method;
      }

      /// <summary>
      /// Returns a client method name for the given endpoint.
      /// </summary>
      /// <param name="endpoint">The endpoint to query the client method name for.</param>
      internal static string GetClientMethodName(this IReflectedEndpoint endpoint) => endpoint.Name;

      // Utility to build a parameter list separated by comma.
      private static string GetEncodeCallParameterList(CodeParameterDeclarationExpressionCollection parameters)
      {
         var parameterList = new List<string>();
         foreach (CodeParameterDeclarationExpression parameter in parameters)
         {
            parameterList.Add(parameter.Name);
         }

         return string.Join(", ", parameterList);
      }
   }
}

using Ajuna.DotNet.Client.Interfaces;
using System;
using System.CodeDom;
using System.Linq;

namespace Ajuna.DotNet.Extensions
{
   /// <summary>
   /// Simplifies access to ReflectedEndpointRequest interface.
   /// </summary>
   internal static class ReflectedEndpointRequestExtensions
   {
      /// <summary>
      /// Returns a collection of method parameter code declarations.
      /// </summary>
      /// <param name="request">The request to query the parameter for.</param>
      /// <param name="currentNamespace">The current namespace where the generated class will be attached to.</param>
      /// <returns></returns>
      internal static CodeParameterDeclarationExpressionCollection ToInterfaceMethodParameters(this IReflectedEndpointRequest request)
      {
         var result = new CodeParameterDeclarationExpressionCollection();

         var parameterList = request.GetParameters().ToList();
         if (parameterList.Count == 0)
         {
            return result;
         }

         if (parameterList.Count > 1)
         {
            throw new NotImplementedException();
         }

         // Get the parameter.
         IReflectedEndpointNamedType parameter = parameterList[0];

         // All parameters are generated with "string key" at this point.
         // Once the Rest Service learns new parameters we have to update the client generator accordingly. We cannot
         // predict at this point what may will come in future.
         if (parameter.Name != "key" || parameter.Type != typeof(string))
         {
            throw new NotImplementedException();
         }

         // The key parameter is an encoded parameter depending on the controller storage access implementation.
         // TODO (svnscha): Try to get the underlying storage access and build a user friendly type.

         result.Add(new CodeParameterDeclarationExpression(parameter.Type, parameter.Name));
         return result;
      }
   }
}

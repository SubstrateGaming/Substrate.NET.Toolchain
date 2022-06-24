using Ajuna.DotNet.Client.Interfaces;
using System.CodeDom;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Ajuna.DotNet.Extensions
{
   /// <summary>
   /// Simplifies access to ReflectedEndpointResponse interface.
   /// </summary>
   internal static class ReflectedEndpointResponseExtensions
   {
      /// <summary>
      /// Returns a code reference element that holds the actual return type of an client interface method.
      /// </summary>
      /// <param name="response">The response object to get the return type for.</param>
      /// <param name="currentNamespace">The current namespace where the generated class will be attached to.</param>
      /// <returns></returns>
      internal static CodeTypeReference ToInterfaceMethodReturnType(this IReflectedEndpointResponse response, CodeNamespace currentNamespace)
      {
         IReflectedEndpointType defaultReturnType = response.GetSuccessReturnType();

         if (defaultReturnType == null)
         {
            return new CodeTypeReference(typeof(void));
         }

         // Ensure we are importing all model items.
         // Not actually required since we use fully qualified items but we want to get rid of that later.
         currentNamespace.Imports.Add(new CodeNamespaceImport(defaultReturnType.Type.Namespace));

         return new CodeTypeReference(typeof(Task<>).MakeGenericType(new[] { defaultReturnType.Type }));
      }

      /// <summary>
      /// Returns a matching type for HttpStatusCode.OK.
      /// </summary>
      /// <param name="response">The response object to get the return type for.</param>
      internal static IReflectedEndpointType GetSuccessReturnType(this IReflectedEndpointResponse response)
      {
         Dictionary<int, IReflectedEndpointType> possibleReturnTypes = response.GetReturnTypesByStatusCode();
         if (possibleReturnTypes.ContainsKey((int)HttpStatusCode.OK))
         {
            // We assume that the default http status code 200 is the default wanted return type.
            return possibleReturnTypes[(int)HttpStatusCode.OK];
         }

         return null;
      }
   }
}

using Ajuna.DotNet.Client.Interfaces;
using Ajuna.DotNet.Client.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Reflection;

namespace Ajuna.DotNet.Extensions
{
   /// <summary>
   /// Simplifies access to MethodInfo
   /// </summary>
   internal static class MethodInfoExtensions
   {
      /// <summary>
      /// Returns true if the given method has a return type of IActionResult.
      /// </summary>
      /// <param name="method">The method to check.</param>
      internal static bool HasActionResultReturnType(this MethodInfo method)
      {
         return method.ReturnType == typeof(IActionResult);
      }

      /// <summary>
      /// Returns true if the given method has exactly one [HttpGet]-Attribute attached.
      /// </summary>
      /// <param name="method">The method to check.</param>
      internal static bool HasHttpGetAttribute(this MethodInfo method)
      {
         return method.GetCustomAttributes(typeof(HttpGetAttribute), false).Length == 1;
      }

      /// <summary>
      /// Returns the attached [HttpGet] attribute of a given method.
      /// </summary>
      /// <param name="method">The method to query the [HttpGet] attribute for.</param>
      internal static string HttpGetEndpoint(this MethodInfo method)
      {
         return method.GetCustomAttribute<HttpGetAttribute>().Template;
      }

      /// <summary>
      /// Returns true if the given method has exactly one or more [ProducesResponseType]-Attribute attached.
      /// </summary>
      /// <param name="method">The method to check.</param>
      internal static bool HasProducesResponseTypeAttribute(this MethodInfo method)
      {
         return method.GetCustomAttributes(typeof(ProducesResponseTypeAttribute), false).Length >= 1;
      }

      /// <summary>
      /// Returns a dictionary containing HttpStatusCode values as key and [ProducesResponseType]-Attributes as value for the given method.
      /// </summary>
      /// <param name="method">The method to query the [ProducesResponseType] attributes for.</param>
      internal static Dictionary<int, IReflectedEndpointType> GetProducedReturnType(this MethodInfo method)
      {
         if (!method.HasProducesResponseTypeAttribute())
            return new Dictionary<int, IReflectedEndpointType>();

         var result = new Dictionary<int, IReflectedEndpointType>();
         foreach (var attribute in method.GetCustomAttributes<ProducesResponseTypeAttribute>(false))
         {
            result.Add(attribute.StatusCode, new ReflectedEndpointType(attribute.Type));
         }
         return result;
      }
   }
}

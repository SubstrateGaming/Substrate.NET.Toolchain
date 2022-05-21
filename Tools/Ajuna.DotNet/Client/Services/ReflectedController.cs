using Ajuna.DotNet.Client.Interfaces;
using Ajuna.DotNet.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Ajuna.DotNet.Client.Services
{
   /// <summary>
   /// Implements controller reflection.
   /// </summary>
   internal class ReflectedController : IReflectedController
   {
      private readonly Type _controllerType;

      /// <summary>
      /// Constructs a reflected controller instance using the given controller type.
      /// </summary>
      /// <param name="controllerType">The controller type to reflect.</param>
      public ReflectedController(Type controllerType)
      {
         _controllerType = controllerType;
      }

      /// <summary>
      /// Returns the class name of the given controller instance.
      /// </summary>
      public string Name => _controllerType.Name;

      /// <summary>
      /// Reflects this controller instance and builds an enumerable set of endpoints.
      /// </summary>
      /// <remarks>This is currently limited to HTTP Get requests.</remarks>
      /// <returns>Returns an enumerable set of reflected endpoints.</returns>
      public IEnumerable<IReflectedEndpoint> GetEndpoints()
      {
         var methods = _controllerType
            .GetMethods()
            .Where(method => method.IsPublic && method.HasActionResultReturnType())
            .AsQueryable();

         // Process all HTTP Get methods.
         var httpGetMethods = methods.Where(method => method.HasHttpGetAttribute())
            .Select(method => new ReflectedEndpoint(method, HttpMethod.Get))
            .ToList();

         // Yield the results for enumerable.
         foreach (var method in httpGetMethods)
            yield return method;

         // Clear the actual list.
         httpGetMethods.Clear();
      }

      /// <summary>
      /// Returns the controller name.
      /// </summary>
      public override string ToString() => Name;
   }
}
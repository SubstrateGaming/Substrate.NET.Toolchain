using Ajuna.RestService.ClientGenerator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace Ajuna.RestService.ClientGenerator.Services
{
   /// <summary>
   /// Implements a reflected endpoint request.
   /// </summary>
   internal abstract class ReflectedEndpointRequest : IReflectedEndpointRequest
   {
      private readonly MethodInfo _endpointMethod;

      /// <summary>
      /// The HTTP method.
      /// </summary>
      public abstract HttpMethod HttpMethod { get; }

      /// <summary>
      /// The Endpoint URL.
      /// </summary>
      public abstract string Endpoint { get; }

      /// <summary>
      /// Constructs an endpoint request using the given method.
      /// </summary>
      /// <param name="methodInfo">The method to parse the actual request items.</param>
      protected ReflectedEndpointRequest(MethodInfo methodInfo)
      {
         _endpointMethod = methodInfo;
      }

      /// <summary>
      /// Returns an enumerable set of request parameters.
      /// </summary>
      /// <returns></returns>
      /// <exception cref="NotImplementedException">Throws an exception if a parameter has Attributes attached.</exception>
      public IEnumerable<IReflectedEndpointNamedType> GetParameters()
      {
         var parameters = _endpointMethod.GetParameters();

         // The current generated controllers do not support custom attributes in parameters.
         // But if it does we better bail here to make sure that'll get properly implemented once needed.
         if (parameters.Any(x => x.CustomAttributes.Any()))
            throw new NotImplementedException();

         return parameters.Select(parameter => new ReflectedEndpointNamedType(parameter.ParameterType, parameter.Name)).ToList();
      }
   }
}
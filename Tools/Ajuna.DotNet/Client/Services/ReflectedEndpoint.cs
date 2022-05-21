using Ajuna.DotNet.Client.Interfaces;
using System;
using System.Net.Http;
using System.Reflection;

namespace Ajuna.DotNet.Client.Services
{
   /// <summary>
   /// Implements endpoint reflection.
   /// </summary>
   internal class ReflectedEndpoint : IReflectedEndpoint
   {
      private readonly MethodInfo _endpointMethod;
      private readonly IReflectedEndpointRequest _endpointRequest;

      /// <summary>
      /// Constructs a reflected endpoint instance using the given method and its httpMethod.
      /// </summary>
      public ReflectedEndpoint(MethodInfo method, HttpMethod httpMethod)
      {
         _endpointMethod = method;

         if (httpMethod == HttpMethod.Get)
         {
            // Get Methods are the only currently supported methods because the Rest Service does not yet generate any other kind of method.
            _endpointRequest = new ReflectedEndpointGetRequest(method);
         }
      }

      /// <summary>
      /// The method name.
      /// </summary>
      public string Name => _endpointMethod.Name;

      /// <summary>
      /// Returns the Endpoint URL.
      /// </summary>
      public string Endpoint => _endpointRequest.Endpoint;

      /// <summary>
      /// Returns a reflected endpoint request.
      /// </summary>
      /// <exception cref="NotImplementedException">Throws if the current method is not yet supported.</exception>
      public IReflectedEndpointRequest GetRequest()
      {
         if (_endpointRequest == null)
         {
            // A null endpoint request represents a method that isn't supported.
            throw new NotImplementedException();
         }

         return _endpointRequest;
      }

      /// <summary>
      /// Returns a reflected endpoint response.
      /// </summary>
      /// <returns></returns>
      public IReflectedEndpointResponse GetResponse()
      {
         return new ReflectedEndpointResponse(_endpointMethod);
      }

      /// <summary>
      /// Returns the Endpoint URL.
      /// </summary>
      public override string ToString() => Endpoint;
   }
}
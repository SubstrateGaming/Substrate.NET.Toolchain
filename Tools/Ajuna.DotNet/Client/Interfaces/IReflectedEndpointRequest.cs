using Ajuna.ServiceLayer.Attributes;
using System.Collections.Generic;
using System.Net.Http;

namespace Ajuna.DotNet.Client.Interfaces
{
   /// <summary>
   /// Interface that represents a REST Service controller method request.
   /// </summary>
   internal interface IReflectedEndpointRequest
   {
      /// <summary>
      /// The HTTP Method for this endpoint request.
      /// </summary>
      HttpMethod HttpMethod { get; }

      /// <summary>
      /// The Endpoint URL.
      /// </summary>
      string Endpoint { get; }

      /// <summary>
      /// The KeyBuilder attribute that gives a hint how to build the encoded Key parameter.
      /// </summary>
      StorageKeyBuilderAttribute KeyBuilderAttribute { get; }
      
      /// <summary>
      /// Enumerable set of request parameters such as query parameters.
      /// </summary>
      IEnumerable<IReflectedEndpointNamedType> GetParameters();

   }
}

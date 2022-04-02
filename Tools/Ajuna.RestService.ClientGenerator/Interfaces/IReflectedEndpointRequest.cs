using System.Collections.Generic;
using System.Net.Http;

namespace Ajuna.RestService.ClientGenerator.Interfaces
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
      /// The route endpoint URL.
      /// </summary>
      string Endpoint { get; }

      /// <summary>
      /// Enumerable set of request parameters such as query parameters.
      /// </summary>
      IEnumerable<IReflectedEndpointNamedType> GetParameters();
   }
}

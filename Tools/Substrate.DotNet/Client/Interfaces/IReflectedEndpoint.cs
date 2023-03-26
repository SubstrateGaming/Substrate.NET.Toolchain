namespace Substrate.DotNet.Client.Interfaces
{
   /// <summary>
   /// Interface that represents a REST Service controller method.
   /// </summary>
   internal interface IReflectedEndpoint
   {
      /// <summary>
      /// The method name.
      /// </summary>
      string Name { get; }

      /// <summary>
      /// The route endpoint URL.
      /// </summary>
      string Endpoint { get; }

      /// <summary>
      /// The request parameters.
      /// </summary>
      IReflectedEndpointRequest GetRequest();

      /// <summary>
      /// The response parameter.
      /// </summary>
      IReflectedEndpointResponse GetResponse();
   }
}

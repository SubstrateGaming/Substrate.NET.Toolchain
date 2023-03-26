namespace Substrate.DotNet.Client.Interfaces
{
   /// <summary>
   /// Interface that represents a REST Service controller method request parameters such as query parameters.
   /// </summary>
   internal interface IReflectedEndpointNamedType : IReflectedEndpointType
   {
      /// <summary>
      /// Parameter name
      /// </summary>
      string Name { get; }
   }
}

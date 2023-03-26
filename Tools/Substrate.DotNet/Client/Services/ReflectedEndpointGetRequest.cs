using Substrate.DotNet.Extensions;
using Substrate.ServiceLayer.Attributes;
using System.Net.Http;
using System.Reflection;

namespace Substrate.DotNet.Client.Services
{
   /// <summary>
   /// Implements endpoint Http GET request reflection.
   /// </summary>
   internal class ReflectedEndpointGetRequest : ReflectedEndpointRequest
   {
      private readonly string _endpoint;
      private readonly StorageKeyBuilderAttribute _keyBuilderAttribute;

      /// <summary>
      /// Constructs a new ReflectedEndpointGetRequest instance and extracts all [HttpGet] attribute parameters that we need
      /// for proper client generation.
      /// </summary>
      /// <param name="methodInfo"></param>
      internal ReflectedEndpointGetRequest(MethodInfo methodInfo) : base(methodInfo)
      {
         _endpoint = methodInfo.HttpGetEndpoint();
         _keyBuilderAttribute = methodInfo.ExtractKeyBuilderAttribute();
      }

      /// <summary>
      /// Returns always HttpMethod.Get
      /// </summary>
      public override HttpMethod HttpMethod => HttpMethod.Get;

      /// <summary>
      /// The extracted endpoint template of [HttpGet] attribute.
      /// </summary>
      public override string Endpoint => _endpoint;

      /// <summary>
      /// The extracted key builder attribute.
      /// </summary>
      public override StorageKeyBuilderAttribute KeyBuilderAttribute => _keyBuilderAttribute;
   }
}
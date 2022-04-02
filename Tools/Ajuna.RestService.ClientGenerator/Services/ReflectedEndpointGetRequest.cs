using Ajuna.RestService.ClientGenerator.Extensions;
using System.Net.Http;
using System.Reflection;

namespace Ajuna.RestService.ClientGenerator.Services
{
    /// <summary>
    /// Implements endpoint Http GET request reflection.
    /// </summary>
    internal class ReflectedEndpointGetRequest : ReflectedEndpointRequest
    {
        private readonly string _endpoint;

        /// <summary>
        /// Constructs a new ReflectedEndpointGetRequest instance and extracts all [HttpGet] attribute parameters that we need
        /// for proper client generation.
        /// </summary>
        /// <param name="methodInfo"></param>
        internal ReflectedEndpointGetRequest(MethodInfo methodInfo) : base(methodInfo)
        {
            _endpoint = methodInfo.HttpGetEndpoint();
        }

        /// <summary>
        /// Returns always HttpMethod.Get
        /// </summary>
        public override HttpMethod HttpMethod => HttpMethod.Get;

        /// <summary>
        /// The extracted endpoint template of [HttpGet] attribute.
        /// </summary>
        public override string Endpoint => _endpoint;
    }
}
using Ajuna.RestService.ClientGenerator.Extensions;
using Ajuna.RestService.ClientGenerator.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ajuna.RestService.ClientGenerator.Services
{
    /// <summary>
    /// Implements a reflected endpoint response.
    /// </summary>
    internal class ReflectedEndpointResponse : IReflectedEndpointResponse
    {
        private readonly Dictionary<int, IReflectedEndpointType> _returnType;

        /// <summary>
        /// Constructs an endpoint response using the given method.
        /// </summary>
        /// <param name="methodInfo">The method to parse the actual request items.</param>
        internal ReflectedEndpointResponse(MethodInfo methodInfo)
        {
            _returnType = methodInfo.GetProducedReturnType();
        }

        /// <summary>
        /// Returns a dictionary that holds HTTP status codes and its response type.
        /// </summary>
        public Dictionary<int, IReflectedEndpointType> GetReturnTypesByStatusCode()
        {
            return _returnType;
        }

        /// <summary>
        /// Returns a comma separated list of status code and its return type.
        /// </summary>
        public override string ToString()
        {
            return string.Join(", ", _returnType.Select(kvp => $"{kvp.Key}: {kvp.Value}").ToList());
        }
    }
}
using System;

namespace Substrate.DotNet.Client.Interfaces
{
   /// <summary>
   /// Interface that represents a REST Service controller method response type.
   /// Currently just holds the actual type.
   /// </summary>
   internal interface IReflectedEndpointType
   {
      /// <summary>
      /// Holds the actual type.
      /// </summary>
      Type Type { get; }
   }
}

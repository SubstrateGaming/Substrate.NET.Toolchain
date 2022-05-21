using System.Collections.Generic;

namespace Ajuna.DotNet.Client.Interfaces
{
   /// <summary>
   /// Interface that represents a REST Service controller.
   /// </summary>
   internal interface IReflectedController
   {
      /// <summary>
      /// The controller name.
      /// </summary>
      string Name { get; }

      /// <summary>
      /// An enumerable set of controller methods (endpoints).
      /// </summary>
      IEnumerable<IReflectedEndpoint> GetEndpoints();
   }
}

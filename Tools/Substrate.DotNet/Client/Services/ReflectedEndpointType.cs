using Substrate.DotNet.Client.Interfaces;
using System;

namespace Substrate.DotNet.Client.Services
{
   /// <summary>
   /// Implements a reflected endpoint type.
   /// </summary>
   internal class ReflectedEndpointType : IReflectedEndpointType
   {
      private readonly Type _type;

      /// <summary>
      /// Constructs a reflected endpoint type using the given type.
      /// </summary>
      /// <param name="type">The actual underlying type.</param>
      public ReflectedEndpointType(Type type)
      {
         _type = type;
      }

      /// <summary>
      /// Holds the actual type.
      /// </summary>
      public Type Type => _type;

      /// <summary>
      /// Returns the type name.
      /// </summary>
      public override string ToString() => Type.Name;
   }
}
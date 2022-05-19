using Ajuna.RestService.ClientGenerator.Interfaces;
using System;

namespace Ajuna.RestService.ClientGenerator.Services
{
   /// <summary>
   /// Implements endpoint named parameters.
   /// </summary>
   internal class ReflectedEndpointNamedType : IReflectedEndpointNamedType
   {
      private readonly Type _type;
      private readonly string _name;

      /// <summary>
      /// Constructs a new parameter with the given type and name.
      /// </summary>
      /// <param name="type">The type.</param>
      /// <param name="name">The parameter name.</param>
      public ReflectedEndpointNamedType(Type type, string name)
      {
         _type = type;
         _name = name;
      }

      /// <summary>
      /// Returns the parameter type.
      /// </summary>
      public Type Type => _type;

      /// <summary>
      /// Returns the parameter name.
      /// </summary>
      public string Name => _name;

      /// <summary>
      /// Returns a combination of parameter type and name in the following format [Type] [Name].
      /// </summary>
      public override string ToString() => $"{Type} {Name}";
   }
}
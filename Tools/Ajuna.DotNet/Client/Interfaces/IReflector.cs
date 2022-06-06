using System;
using System.Collections.Generic;
using System.Reflection;

namespace Ajuna.DotNet.Client.Interfaces
{
   /// <summary>
   /// Base reflector interface.
   /// </summary>
   internal interface IReflector
   {
      /// <summary>
      /// Returns an enumerable set of controllers in the given assembly while scanning for all classes that inerhit from the given base type.
      /// </summary>
      /// <param name="assembly">The assembly to check for.</param>
      /// <param name="baseType">The base type that a controller class must inherit from.</param>
      IEnumerable<IReflectedController> GetControllers(Assembly assembly, Type baseType);
   }
}

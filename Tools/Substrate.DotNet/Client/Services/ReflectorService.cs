using Substrate.DotNet.Client.Interfaces;
using Substrate.ServiceLayer.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Substrate.DotNet.Client.Services
{
   /// <summary>
   /// ReflectorService is the entry point to iterate and query over the controller classes
   /// of a given assembly.
   /// </summary>
   internal class ReflectorService : IDisposable, IReflector
   {
      /// <summary>
      /// Convenience
      /// </summary>
      public void Dispose()
      {
      }

      /// <summary>
      /// Returns an enumerable set of controllers in the given assembly while scanning for all classes that inerhit from the given base type.
      /// </summary>
      /// <param name="assembly">The assembly to check for.</param>
      /// <param name="baseType">The base type that a controller class must inherit from.</param>
      public IEnumerable<IReflectedController> GetControllers(Assembly assembly, Type baseType)
      {
         return assembly
            .GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(baseType))
            .Where(type => !type.CustomAttributes.Any(x => x.AttributeType == typeof(AjunaControllerIgnoreAttribute)))
            .Select(controllerType => new ReflectedController(controllerType) { })
            .ToList();
      }
   }
}

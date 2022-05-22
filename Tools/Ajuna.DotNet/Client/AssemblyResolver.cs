using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Ajuna.DotNet.Client
{
   /// <summary>
   /// Utility to make sure we can load Ajuna.RestService.dll with its dependencies.
   /// Reference: https://github.com/dotnet/runtime/issues/1050
   /// </summary>
   internal sealed class AssemblyResolver : IDisposable
   {
      private readonly AssemblyLoadContext _loadContext;
      private readonly DependencyContext _dependencyContext;
      private readonly ICompilationAssemblyResolver _assemblyResolver;

      public AssemblyResolver(string path)
      {
         _loadContext = new AssemblyLoadContext(path, true);

         Assembly = _loadContext.LoadFromAssemblyPath(path);

         _dependencyContext = DependencyContext.Load(Assembly);
         _assemblyResolver = new CompositeCompilationAssemblyResolver(new ICompilationAssemblyResolver[]
         {
            // Probe the app's bin folder.
            new AppBaseCompilationAssemblyResolver(Path.GetDirectoryName(path)),

            // Probe reference assemblies.
            new ReferenceAssemblyPathResolver(),

            // Probe NuGet package cache
            new PackageCompilationAssemblyResolver(),
         });

         _loadContext.Resolving += OnResolving;
      }

      public Assembly Assembly { get; }

      public void Dispose()
      {
         _loadContext.Resolving -= OnResolving;
         _loadContext.Unload();
      }

      /// <summary>
      /// Called when the assembly's dependencies couldn't be loaded.
      /// </summary>
      /// <param name="context">The context.</param>
      /// <param name="name">The assembly.</param>
      /// <returns>Returns the resolved dependent assembly otherwise null.</returns>
      private Assembly OnResolving(AssemblyLoadContext context, AssemblyName name)
      {
         // Try to resolve the dependent assembly by looking in the dependency ({assembly}.deps.json) file.
         CompilationLibrary compileLibrary = _dependencyContext.CompileLibraries
             .FirstOrDefault(x => x.Name.Equals(name.Name, StringComparison.OrdinalIgnoreCase));

         if (compileLibrary == null || compileLibrary.Assemblies.Count == 0)
         {
            // If the application has PreserveCompilationContext set to 'false' we also need to check runtime libraries.
            // This shouldn't be the case with projects using Microsoft.NET.Sdk.Web, which defaults to 'true'.
            RuntimeLibrary runtimeLibrary = _dependencyContext.RuntimeLibraries
                .FirstOrDefault(x => x.Name.Equals(name.Name, StringComparison.OrdinalIgnoreCase));

            if (runtimeLibrary != null)
            {
               compileLibrary = new CompilationLibrary(
                   runtimeLibrary.Type,
                   runtimeLibrary.Name,
                   runtimeLibrary.Version,
                   runtimeLibrary.Hash,
                   runtimeLibrary.RuntimeAssemblyGroups.SelectMany(g => g.AssetPaths),
                   runtimeLibrary.Dependencies,
                   runtimeLibrary.Serviceable);
            }
         }

         if (compileLibrary == null)
         {
            return null;
         }

         var assemblyPaths = new List<string>();

         if (_assemblyResolver.TryResolveAssemblyPaths(compileLibrary, assemblyPaths))
         {
            try
            {
               if (assemblyPaths.Count == 0)
               {
                  return null;
               }

               return _loadContext.LoadFromAssemblyPath(assemblyPaths.First());
            }
            catch
            {
            }
         }
         return null;
      }
   }
}

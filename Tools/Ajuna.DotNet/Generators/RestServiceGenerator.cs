using Ajuna.DotNet.Generators.Base;
using Ajuna.DotNet.Node;
using Ajuna.NetApi.Model.Meta;
using Serilog;

namespace Ajuna.DotNet.Generators
{
   /// <summary>
   /// Responsible for generating the RestService Solution
   /// </summary>
   public class RestServiceGenerator : SolutionGeneratorBase
   {
      // private readonly DotNetCli _dotNetSolutionGenerator;

      public RestServiceGenerator(ILogger logger, string nodeRuntime, ProjectSettings projectSettings)
         : base(logger, nodeRuntime, projectSettings)
      {
         // _dotNetSolutionGenerator = new DotNetCli(logger, projectSettings.ProjectDirectory);
      }

      protected override void GenerateClasses(MetaData metadata)
      {        
         GetGenericStructs(metadata.NodeMetadata.Types);

         // Generate types as if we were generating them for Types project but just keep them in memory
         // so we can reference these types and we don't output all the types while generating the rest service.
         var typeDict = GenerateTypes(metadata.NodeMetadata.Types, ProjectSettings.ProjectDirectory, write: false);

         foreach (var module in metadata.NodeMetadata.Modules.Values)
         {
            RestServiceStorageModuleBuilder
                .Init(module.Index, module, typeDict, metadata.NodeMetadata.Types)
                .Create()
                .Build(write: true, out bool _, basePath: ProjectSettings.ProjectDirectory);

            RestServiceControllerModuleBuilder
                .Init(module.Index, module, typeDict, metadata.NodeMetadata.Types)
                .Create()
                .Build(write: true, out bool _, basePath: ProjectSettings.ProjectDirectory);
         }
      }

   }
}
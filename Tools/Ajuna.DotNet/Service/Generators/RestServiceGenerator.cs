using Ajuna.DotNet.Service.Generators.Base;
using Ajuna.DotNet.Service.Node;
using Ajuna.NetApi.Model.Meta;
using Serilog;

namespace Ajuna.DotNet.Service.Generators
{
   /// <summary>
   /// Responsible for generating the RestService Solution
   /// </summary>
   public class RestServiceGenerator : SolutionGeneratorBase
   {
      private readonly ProjectSettings _projectSettings;

      public RestServiceGenerator(ILogger logger, string nodeRuntime, string netApiProjectName, ProjectSettings projectSettings)
         : base(logger, nodeRuntime, netApiProjectName)
      {
         // Rest Service project configuration.
         _projectSettings = projectSettings;
      }

      protected override void GenerateClasses(MetaData metadata)
      {
         GetGenericStructs(metadata.NodeMetadata.Types);

         // Generate types as if we were generating them for Types project but just keep them in memory
         // so we can reference these types and we don't output all the types while generating the rest service.
         var typeDict = GenerateTypes(metadata.NodeMetadata.Types, string.Empty, write: false);

         foreach (var module in metadata.NodeMetadata.Modules.Values)
         {
            RestServiceStorageModuleBuilder
                .Init(_projectSettings.ProjectName, module.Index, module, typeDict, metadata.NodeMetadata.Types)
                .Create()
                .Build(write: true, out bool _, basePath: _projectSettings.ProjectDirectory);

            RestServiceControllerModuleBuilder
                .Init(_projectSettings.ProjectName, module.Index, module, typeDict, metadata.NodeMetadata.Types)
                .Create()
                .Build(write: true, out bool _, basePath: _projectSettings.ProjectDirectory);
         }
      }

   }
}
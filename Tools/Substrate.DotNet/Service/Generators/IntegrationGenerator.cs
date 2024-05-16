using Substrate.DotNet.Service.Generators.Base;
using Substrate.DotNet.Service.Node;
using Substrate.NetApi.Model.Meta;
using Serilog;
using System.Collections.Generic;

namespace Substrate.DotNet.Service.Generators
{
   /// <summary>
   /// Responsible for generating the NetApi Solution
   /// </summary>
   public class IntegrationGenerator : SolutionGeneratorBase
   {
      private readonly string _nodeRuntime;
      private readonly ProjectSettings _projectSettings;

      public IntegrationGenerator(ILogger logger, string nodeRuntime, ProjectSettings projectSettings) : base(logger, nodeRuntime, projectSettings.ProjectName)
      {
         _nodeRuntime = nodeRuntime;
         _projectSettings = projectSettings;
      }

      protected override void GenerateClasses(MetaData metadata)
      {
         GenerateIntegration(metadata.NodeMetadata.Modules, metadata);
      }

      private void GenerateIntegration(Dictionary<uint, PalletModule> modules, MetaData metadata)
      {
         List<string> modulesResolved = new();
         foreach (PalletModule module in modules.Values) { }

         BaseClientBuilder
          .Init(_projectSettings.ProjectName, 0, modulesResolved, null, metadata).Create()
          .Build(write: true, out bool _, _projectSettings.ProjectDirectory);

         ExtrinsicManagerBuilder
          .Init(_projectSettings.ProjectName, 0, modulesResolved, null).Create()
          .Build(write: true, out bool _, _projectSettings.ProjectDirectory);

         ExtrinsicInfoBuilder
          .Init(_projectSettings.ProjectName, 0, modulesResolved, null, metadata, _nodeRuntime).Create()
          .Build(write: true, out bool _, _projectSettings.ProjectDirectory);

        SubscriptionManagerBuilder
          .Init(_projectSettings.ProjectName, 0, modulesResolved, null).Create()
          .Build(write: true, out bool _, _projectSettings.ProjectDirectory);

         GenericHelperBuilder
          .Init(_projectSettings.ProjectName, 0, modulesResolved, null).Create()
          .Build(write: true, out bool _, _projectSettings.ProjectDirectory);
      }
   }
}
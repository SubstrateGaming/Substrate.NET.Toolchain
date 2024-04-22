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
      private readonly ProjectSettings _projectSettings;

      public IntegrationGenerator(ILogger logger, string nodeRuntime, ProjectSettings projectSettings) : base(logger, nodeRuntime, projectSettings.ProjectName)
      {
         _projectSettings = projectSettings;
      }

      protected override void GenerateClasses(MetaData metadata)
      {
         GenerateIntegration(metadata.NodeMetadata.Modules);
      }

      private void GenerateIntegration(Dictionary<uint, PalletModule> modules)
      {
         List<string> modulesResolved = new();
         foreach (PalletModule module in modules.Values)
         {

         }

         BaseClientBuilder
          .Init(_projectSettings.ProjectName, 0, modulesResolved, null).Create()
          .Build(write: true, out bool _, _projectSettings.ProjectDirectory);
      }
   }
}
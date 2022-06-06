using Ajuna.DotNet.Service.Generators.Base;
using Ajuna.DotNet.Service.Node;
using Ajuna.NetApi.Model.Meta;
using Serilog;
using System.Collections.Generic;

namespace Ajuna.DotNet.Service.Generators
{
   /// <summary>
   /// Responsible for generating the NetApi Solution
   /// </summary>
   public class NetApiGenerator : SolutionGeneratorBase
   {
      private readonly ProjectSettings _projectSettings;

      public NetApiGenerator(ILogger logger, string nodeRuntime, ProjectSettings projectSettings) : base(logger, nodeRuntime, projectSettings.ProjectName)
      {
         _projectSettings = projectSettings;
      }

      protected override void GenerateClasses(MetaData metadata)
      {
         // dirty workaround for generics.
         // TODO (svnscha) Why dirty workaround?
         SolutionGeneratorBase.GetGenericStructs(metadata.NodeMetadata.Types);

         // generate types
         Dictionary<uint, (string, List<string>)> typeDict = GenerateTypes(metadata.NodeMetadata.Types, _projectSettings.ProjectDirectory, write: true);

         // generate modules
         GenerateModules(ProjectName, metadata.NodeMetadata.Modules, typeDict, metadata.NodeMetadata.Types, _projectSettings.ProjectDirectory);

         // generate base event handler
         // TODO (svnscha) Why disabled?
         //GenerateBaseEvents(metadata.NodeMetadata.Modules, typeDict, metadata.NodeMetadata.Types);
      }

      private static void GenerateModules(string projectName, Dictionary<uint, PalletModule> modules, Dictionary<uint, (string, List<string>)> typeDict, Dictionary<uint, NodeType> nodeTypes, string basePath)
      {
         List<(string, List<string>)> moduleNames = new();
         foreach (PalletModule module in modules.Values)
         {
            (string, List<string>) moduleNameTuple = ModuleGenBuilder
                .Init(projectName, module.Index, module, typeDict, nodeTypes)
                .Create()
                .Build(write: true, out bool _, basePath);

            moduleNames.Add(moduleNameTuple);
         }

         ClientBuilder
             .Init(projectName, 0, moduleNames, typeDict).Create()
             .Build(write: true, out bool _, basePath);
      }
   }
}
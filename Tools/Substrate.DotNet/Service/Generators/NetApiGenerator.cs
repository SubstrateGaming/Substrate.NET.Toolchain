using Substrate.DotNet.Service.Generators.Base;
using Substrate.DotNet.Service.Node;
using Substrate.NetApi.Model.Meta;
using Serilog;
using System.Collections.Generic;
using System;

namespace Substrate.DotNet.Service.Generators
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
         GetGenericStructs(metadata.NodeMetadata.Types);

         // generate types
         NodeTypeResolver typeDict = GenerateTypes(metadata.NodeMetadata.Types, _projectSettings.ProjectDirectory, write: true);

         // generate modules
         GenerateModules(ProjectName, metadata, typeDict, _projectSettings.ProjectDirectory);

         // generate base event handler
         // TODO (svnscha) Why disabled?
         // GenerateBaseEvents(metadata.NodeMetadata.Modules, typeDict, metadata.NodeMetadata.Types);
      }

      private static void GenerateModules(string projectName, MetaData metadata, NodeTypeResolver typeDict, string basePath)
      {
         List<string> modulesResolved = new();
         foreach (PalletModule module in metadata.NodeMetadata.Modules.Values)
         {
            ModuleGenBuilder
                .Init(projectName, module.Index, module, typeDict, metadata.NodeMetadata.Types)
                .Create()
                .Build(write: true, out bool _, basePath);

            modulesResolved.Add($"{module.Name}Storage");
         }

         ClientBuilder
             .Init(projectName, 0, modulesResolved, typeDict).Create()
             .Build(write: true, out bool _, basePath);

         BaseClientBuilder
          .Init(projectName, 0, modulesResolved, null, metadata).Create()
          .Build(write: true, out bool _, basePath);

         ExtrinsicManagerBuilder
          .Init(projectName, 0, modulesResolved, null).Create()
          .Build(write: true, out bool _, basePath);

         ExtrinsicInfoBuilder
          .Init(projectName, 0, modulesResolved, null, metadata).Create()
          .Build(write: true, out bool _, basePath);

         SubscriptionManagerBuilder
           .Init(projectName, 0, modulesResolved, null).Create()
           .Build(write: true, out bool _, basePath);

         GenericHelperBuilder
          .Init(projectName, 0, modulesResolved, null).Create()
          .Build(write: true, out bool _, basePath);
      }
   }
}
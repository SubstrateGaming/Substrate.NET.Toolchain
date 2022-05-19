using Ajuna.DotNet.Generators.Base;
using Ajuna.DotNet.Node;
using Ajuna.NetApi.Model.Meta;
using Serilog;
using System.IO;

namespace Ajuna.DotNet.Generators
{
   /// <summary>
   /// Responsible for generating the RestService Solution
   /// </summary>
   public class RestServiceGenerator : SolutionGeneratorBase
   {
      private readonly DotNetCli _dotNetSolutionGenerator;

      public RestServiceGenerator(ILogger logger, string nodeRuntime, ProjectSettings projectSettings)
         : base(logger, nodeRuntime, projectSettings)
      {
         _dotNetSolutionGenerator = new DotNetCli(logger, projectSettings.ProjectDirectory);
      }

      protected override void GenerateClasses(MetaData metadata)
      {
         string basePath = Path.Combine(ProjectSettings.ProjectDirectory, ProjectSettings.ProjectName);

         // TODO (svnscha): Double-check why this was dirty and make it not dirty.
         //String basePath = WorkingDirectory;            
         // dirty workaround for generics.
         GetGenericStructs(metadata.NodeMetadata.Types);
         var typeDict = GenerateTypes(metadata.NodeMetadata.Types, basePath);

         foreach (var module in metadata.NodeMetadata.Modules.Values)
         {
            RestStorageModuleBuilder
                .Init(module.Index, module, typeDict, metadata.NodeMetadata.Types)
                .Create()
                .Build(write: true, out bool _, basePath: basePath);

            RestControllerModuleBuilder
                .Init(module.Index, module, typeDict, metadata.NodeMetadata.Types)
                .Create()
                .Build(write: true, out bool _, basePath: basePath);
         }
      }

      protected override void GenerateDotNetSolution()
      {
         // Create Solution 
         _dotNetSolutionGenerator.CreateSolution(ProjectSettings.ProjectSolutionName);

         // Create Lib Project
         _dotNetSolutionGenerator.CreateNetstandard2Project(ProjectSettings.ProjectName, ProjectType.ClassLib);
         _dotNetSolutionGenerator.AddProjectToSolution(ProjectSettings.ProjectName);

         // Add Nuget Packages 
         _dotNetSolutionGenerator.AddNugetToProject(Constants.AjunaNetApiNugetPackage, ProjectSettings.ProjectName);
         _dotNetSolutionGenerator.AddNugetToProject(Constants.AjunaServiceLayerNugetPackage, ProjectSettings.ProjectName);
         _dotNetSolutionGenerator.AddNugetToProject(Constants.MicrosoftAspNetCoreMvcCoreNugetPackage, ProjectSettings.ProjectName);

         // This service needs to be build so that we can generate RESTful clients.
         // TODO (svnscha): Do we really need this here? We should probably just do this whenever we are building RESTful clients.
         _dotNetSolutionGenerator.RestorePackages();
         _dotNetSolutionGenerator.CleanSolution();
         _dotNetSolutionGenerator.BuildSolution();
      }

   }
}
using Ajuna.DotNet.Generators.Base;
using Ajuna.DotNet.Node;
using Ajuna.NetApi.Model.Meta;
using Newtonsoft.Json;
using Serilog;
using System.Collections.Generic;
using System.IO;

namespace Ajuna.DotNet.Generators
{
   /// <summary>
   /// Responsible for generating the NetApi Solution
   /// </summary>
   public class NetApiGenerator : SolutionGeneratorBase
   {
      private readonly DotNetCli _dotNetSolutionGenerator;

      public NetApiGenerator(ILogger logger, string nodeRuntime, ProjectSettings projectSettings) : base(logger, nodeRuntime, projectSettings)
      {
         _dotNetSolutionGenerator = new DotNetCli(logger, projectSettings.ProjectDirectory);
      }

      protected override void GenerateClasses(MetaData metadata)
      {
         var basePath = Path.Combine(ProjectSettings.ProjectDirectory, "Ajuna.NetApiExt");

         // dirty workaround for generics.
         GetGenericStructs(metadata.NodeMetadata.Types);

         // generate types
         var typeDict = GenerateTypes(metadata.NodeMetadata.Types, basePath);

         // generate modules
         GenerateModules(metadata.NodeMetadata.Modules, typeDict, metadata.NodeMetadata.Types, basePath);

         // generate base event handler
         //GenerateBaseEvents(metadata.NodeMetadata.Modules, typeDict, metadata.NodeMetadata.Types);
         //var path = Path.Combine(basePath, "NetApi", "Ajuna");

         //Directory.CreateDirectory(path);

         WriteJsonFile("metadata.json", metadata, basePath);
      }

      protected override void GenerateDotNetSolution()
      {
         var solutionName = "Ajuna.NetApiExt";
         var classProjectName = "Ajuna.NetApiExt";
         var testProjectName = "Ajuna.NetApiExt.Test";


         // Create Solution 
         _dotNetSolutionGenerator.CreateSolution(solutionName);

         // Create Lib Project
         _dotNetSolutionGenerator.CreateNetstandard2Project(classProjectName, ProjectType.ClassLib);
         _dotNetSolutionGenerator.AddProjectToSolution(classProjectName);

         // Create Test Project 
         _dotNetSolutionGenerator.CreateNet5Project(testProjectName, ProjectType.MsTest);
         _dotNetSolutionGenerator.AddProjectToSolution(testProjectName);
         _dotNetSolutionGenerator.AddReferenceToProject(testProjectName, classProjectName);

         // Add Nuget Packages 
         _dotNetSolutionGenerator.AddNugetToProject("Ajuna.NetApi", classProjectName);

         _dotNetSolutionGenerator.RestorePackages();
      }

      private void WriteJsonFile(string fileName, MetaData runtimeMetadata, string basePath = null)
      {
         var jsonFile = JsonConvert.SerializeObject(runtimeMetadata, Formatting.Indented);
         File.WriteAllText(Path.Combine(basePath, fileName), jsonFile);
      }

      private static void GenerateModules(Dictionary<uint, PalletModule> modules,
          Dictionary<uint, (string, List<string>)> typeDict, Dictionary<uint, NodeType> nodeTypes, string basePath)
      {
         List<(string, List<string>)> moduleNames = new();
         foreach (var module in modules.Values)
         {
            var moduleNameTuple = ModuleGenBuilder
                .Init(module.Index, module, typeDict, nodeTypes)
                .Create()
                .Build(write: true, out bool _, basePath);
            moduleNames.Add(moduleNameTuple);
         }

         ClientBuilder
             .Init(0, moduleNames, typeDict).Create()
             .Build(write: true, out bool _, basePath);
      }
   }
}
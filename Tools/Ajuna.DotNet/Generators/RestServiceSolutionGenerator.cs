using Ajuna.DotNet.Generators.Base;
using Ajuna.DotNet.Node;

/* Unmerged change from project 'Ajuna.DotNet (net6.0)'
Before:
using Ajuna.NetApi.Model.Meta;
After:
using Ajuna.NetApi.Model.Meta;
using System;
using System.IO;
*/
using Ajuna.NetApi.Model.Meta;
using System.IO;

namespace Ajuna.DotNet.Generators
{
   /// <summary>
   /// Responsible for generating the RestService Solution
   /// </summary>
   public class RestServiceSolutionGenerator : SolutionGeneratorBase
   {
      private readonly DotNetSolutionGenerator _dotNetSolutionGenerator;

      public RestServiceSolutionGenerator(string nodeRuntime, string workingDirectory) : base(nodeRuntime,
          workingDirectory)
      {
         _dotNetSolutionGenerator = new DotNetSolutionGenerator(workingDirectory);
      }

      protected override void GenerateClasses(MetaData metadata)
      {
         string basePath = Path.Combine(WorkingDirectory, "Ajuna.RestService");

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
         var solutionName = "Ajuna.RestService";
         var classProjectName = "Ajuna.RestService";

         // Create Solution 
         _dotNetSolutionGenerator.CreateSolution(solutionName);

         // Create Lib Project
         _dotNetSolutionGenerator.CreateNetstandard2Project(classProjectName, ProjectType.ClassLib);
         _dotNetSolutionGenerator.AddProjectToSolution(classProjectName);

         // Add Nuget Packages 
         _dotNetSolutionGenerator.AddNugetToProject("Ajuna.NetApi", classProjectName);
         _dotNetSolutionGenerator.AddNugetToProject("Ajuna.ServiceLayer", classProjectName);
         _dotNetSolutionGenerator.AddNugetToProject("Microsoft.AspNetCore.Mvc.Core", classProjectName);

         _dotNetSolutionGenerator.RestorePackages();

         _dotNetSolutionGenerator.CleanSolution();

         _dotNetSolutionGenerator.BuildSolution();
      }

   }
}
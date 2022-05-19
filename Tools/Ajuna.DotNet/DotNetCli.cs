using Serilog;
using System.Diagnostics;
using System.IO;

namespace Ajuna.DotNet
{
   /// <summary>
   /// Dotnet CLI Wrapper for creating VS Solutions and Projects
   /// </summary>
   public class DotNetCli
   {
      private readonly ILogger _logger;
      private readonly string _workingDirectory;

      public DotNetCli(ILogger logger, string workingDirectory)
      {
         _workingDirectory = workingDirectory;
         _logger = logger;
      }

      public void CreateSolution(string solutionName)
      {
         ExecuteCommand($"new sln --name {solutionName}", _workingDirectory);
      }

      public void CreateNetstandard2Project(string projectName, ProjectType projectType)
      {
         CreateProject(projectName, projectType, "netstandard2.0");
      }

      public void CreateNet5Project(string projectName, ProjectType projectType)
      {
         CreateProject(projectName, projectType, "net5.0");
      }

      public void CreateProject(string projectName, ProjectType projectType, string framework)
      {
         ExecuteCommand($"new {projectType.ToString().ToLower()} --framework {framework} --name {projectName} --output {projectName}", _workingDirectory);
      }

      public void AddProjectToSolution(string projectName)
      {
         var path = Path.Combine(_workingDirectory, projectName, $"{projectName}.csproj");
         ExecuteCommand($"sln add {path}", _workingDirectory);
      }

      public void AddNugetToProject(string packageName, string projectName)
      {
         ExecuteCommand($"add package {packageName}", Path.Combine(_workingDirectory, projectName));
      }

      public void RestorePackages()
      {
         ExecuteCommand($"restore", _workingDirectory);
      }

      public void AddReferenceToProject(string projectToAddReferenceTo, string projectToBeReferenced)
      {
         var projectToAddReferenceToPath = Path.Combine(projectToAddReferenceTo, $"{projectToAddReferenceTo}.csproj");
         var projectToBeReferencedPath = Path.Combine(projectToBeReferenced, $"{projectToBeReferenced}.csproj");

         ExecuteCommand($"add {projectToAddReferenceToPath} reference {projectToBeReferencedPath}", _workingDirectory);
      }

      public void CleanSolution()
      {
         ExecuteCommand("clean", _workingDirectory);
      }

      public void BuildSolution()
      {
         ExecuteCommand("build", _workingDirectory);
      }

      private void ExecuteCommand(string arguments, string workingDirectory)
      {
         _logger.Debug("Executing command \"dotnet {arguments}\" in directory \"{directory}\".", arguments, workingDirectory);

         var process = new Process();
         var startInfo = new ProcessStartInfo
         {
            FileName = "dotnet",
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            RedirectStandardOutput = true,
            RedirectStandardError = true
         };

         process.StartInfo = startInfo;
         process.Start();

         string stdout = process.StandardOutput.ReadToEnd();
         string stderr = process.StandardError.ReadToEnd();

         if (!string.IsNullOrEmpty(stdout))
            _logger.Debug($"stdout:\n{stdout}");

         if (!string.IsNullOrEmpty(stderr))
            _logger.Debug($"stderr:\n{stderr}");

         process.WaitForExit();
      }
   }
}

public enum ProjectType
{
   Console,
   ClassLib,
   MsTest
}
﻿using Ajuna.DotNet.Client;
using Ajuna.DotNet.Service.Generators;
using Ajuna.DotNet.Service.Node;
using Ajuna.NetApi.Model.Meta;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ajuna.DotNet
{
   partial class Program
   {
      /// <summary>
      /// Command line utility to easily maintain and scaffold Ajuna SDK related projects.
      /// 
      /// Usage
      /// dotnet ajuna update
      /// 
      /// </summary>
      static async Task Main(string[] args)
      {
         // Initialize logging.
         Log.Logger = new LoggerConfiguration()
          .MinimumLevel.Verbose()
          .WriteTo.Console()
          .CreateLogger();

         try
         {
            for (int i = 0; i < args.Length; i++)
            {
               switch (args[i])
               {
                  // Handles dotnet ajuna update
                  case "update":
                     {
                        if (!await UpdateAjunaEnvironmentAsync(CancellationToken.None))
                        {
                           Log.Error("Updating project did not complete successfully.");
                           Environment.Exit(-1);
                        }
                     }
                     break;

                  // Handles dotnet ajuna upgrade
                  case "upgrade":
                     {
                        if (!await UpgradeAjunaEnvironmentAsync(CancellationToken.None))
                        {
                           Log.Error("Upgrading project did not complete successfully.");
                           Environment.Exit(-1);
                        }
                     }
                     break;

                  default:
                     break;
               }
            }
         }
         catch (InvalidOperationException ex)
         {
            Log.Error(ex, "Could not complete operation!");
            Environment.Exit(-1);
         }
         catch (Exception ex)
         {
            Log.Error(ex, "Unhandled exception!");
            Environment.Exit(-1);
         }

      }

      /// <summary>
      /// Invoked with dotnet ajuna update
      /// This command parses the ajuna project configuration and generates code for all given projects.
      /// </summary>
      /// <returns>Returns true on success, otherwise false.</returns>
      private static async Task<bool> UpdateAjunaEnvironmentAsync(CancellationToken token) => await UpgradeOrUpdateAjunaEnvironmentAsync(fetchMetadata: false, token);

      /// <summary>
      /// Invoked with dotnet ajuna upgrade.
      /// This command first updates the metadata file and then generates all classes again.
      /// </summary>
      /// <returns>Returns true on success, otherwise false.</returns>
      private static async Task<bool> UpgradeAjunaEnvironmentAsync(CancellationToken token) => await UpgradeOrUpdateAjunaEnvironmentAsync(fetchMetadata: true, token);

      /// <summary>
      /// Handles the implementation to update or upgrade an ajuna environment
      /// Upgrading first fetches the metadata and stores it in .ajuna configuration directory.
      /// Then a normal update command is invoked to generate code for all given projects.
      /// </summary>
      /// <param name="token">Cancellation</param>
      /// <param name="fetchMetadata">Controls whether to fetch the metadata (upgrade) or not (update).</param>
      /// <returns>Returns true on success, otherwise false.</returns>
      private static async Task<bool> UpgradeOrUpdateAjunaEnvironmentAsync(bool fetchMetadata, CancellationToken token)
      {
         // Update an existing Ajuna project tree by reading the required configuration file
         // in the current directory in subdirectory .ajuna.
         string configurationFile = ResolveConfigurationFilePath();
         if (!File.Exists(configurationFile))
         {
            Log.Error("The configuration file {file} does not exist! Please create a configuration file so this toolchain can produce correct outputs. You can scaffold the configuration file by creating a new service project with `dotnet new ajuna-service`.");
            return false;
         }

         // Read ajuna-config.json
         AjunaConfiguration configuration = JsonConvert.DeserializeObject<AjunaConfiguration>(File.ReadAllText(configurationFile));
         if (configuration == null)
         {
            Log.Error("Could not parse the configuration file {file}! Please ensure that the configuration file format is correct.", configurationFile);
            return false;
         }

         Log.Information("Using NetApi Project = {name}", configuration.Projects.NetApi);
         Log.Information("Using RestService Project = {name}", configuration.Projects.RestService);
         Log.Information("Using RestClient Project = {name}", configuration.Projects.RestClient);
         Log.Information("Using RestClient.Mockup Project = {name}", configuration.Projects.RestClientMockup);
         Log.Information("Using RestClient.Test Project = {name}", configuration.Projects.RestClientTest);
         Log.Information("Using RestService assembly for RestClient = {assembly}", configuration.RestClientSettings.ServiceAssembly);
         Log.Information("Using Node Runtime = {runtime}", configuration.Metadata.Runtime);

         if (fetchMetadata)
         {
            Log.Information("Using Websocket = {websocket} to fetch metadata", configuration.Metadata.Websocket);

            if (!await GenerateMetadataAsync(configuration.Metadata.Websocket, token))
            {
               Log.Error("Unable to fetch metadata from websocket {websocket}. Aborting.", configuration.Metadata.Websocket);
               return false;
            }
         }

         string metadataFilePath = ResolveMetadataFilePath();
         Log.Information("Using Metadata = {metadataFilePath}", metadataFilePath);

         MetaData metadata = GetMetadata.GetMetadataFromFile(Log.Logger, metadataFilePath);
         if (metadata == null)
         {
            return false;
         }

         // Service
         GenerateNetApiClasses(metadata, configuration);
         GenerateRestServiceClasses(metadata, configuration);

         // Client
         GenerateRestClientClasses(configuration);

         return true;
      }

      /// <summary>
      /// Fetches and generates .ajuna/metadata.json
      /// </summary>
      /// <param name="websocket">The websocket to connect to</param>
      /// <param name="token">Cancellation token.</param>
      /// <returns>Returns true on success, otherwise false.</returns>
      /// <exception cref="InvalidOperationException"></exception>
      private static async Task<bool> GenerateMetadataAsync(string websocket, CancellationToken token)
      {
         string metadata = await GetMetadata.GetMetadataFromNodeAsync(Log.Logger, websocket, token);
         if (metadata == null)
         {
            throw new InvalidOperationException($"Could not query metadata from node {websocket}!");
         }

         string metadataFilePath = ResolveMetadataFilePath();

         try
         {
            Log.Information("Saving metadata to {metadataFilePath}...", metadataFilePath);
            File.WriteAllText(metadataFilePath, metadata);
            return true;
         }
         catch (Exception e)
         {
            Log.Error(e, $"Could not save metadata to filepath: {metadataFilePath}!");
         }

         return false;
      }

      /// <summary>
      /// Generates all classes for the RestService project
      /// </summary>
      private static void GenerateRestServiceClasses(MetaData metadata, AjunaConfiguration configuration)
      {
         var generator = new RestServiceGenerator(Log.Logger, configuration.Metadata.Runtime, configuration.Projects.NetApi, new ProjectSettings(configuration.Projects.RestService));
         generator.Generate(metadata);
      }

      private static void GenerateRestClientClasses(AjunaConfiguration configuration)
      {
         string filePath = ResolveRestServiceAssembly(configuration);
         if (string.IsNullOrEmpty(filePath))
         {
            Log.Information("Could not resolve RestService assembly file path. Please build the RestService before generating RestClient project classes.");
            return;
         }

         Log.Information("Using resolved RestService assembly for RestClient = {assembly}", filePath);

         using var loader = new AssemblyResolver(filePath);

         // Initialize configuration.
         var clientConfiguration = new ClientGeneratorConfiguration()
         {
            Assembly = loader.Assembly,
            ControllerBaseType = typeof(ControllerBase),
            OutputDirectory = Path.Join(Environment.CurrentDirectory, configuration.Projects.RestClient),
            GeneratorOptions = new CodeGeneratorOptions()
            {
               BlankLinesBetweenMembers = false,
               BracingStyle = "C",
               IndentString = "   "
            },
            BaseNamespace = configuration.Projects.RestClient
         };

         // Build and execute the client generator.
         var client = new ClientGenerator(clientConfiguration);
         client.Generate(Log.Logger);

         // Mockup client.
         clientConfiguration.OutputDirectory = Path.Join(Environment.CurrentDirectory, configuration.Projects.RestClientMockup);
         clientConfiguration.BaseNamespace = configuration.Projects.RestClientMockup;
         clientConfiguration.ClientClassname = "MockupClient";

         var mockupClient = new MockupClientGenerator(clientConfiguration);
         mockupClient.Generate(Log.Logger);

         // Unit test.
         clientConfiguration.OutputDirectory = Path.Join(Environment.CurrentDirectory, configuration.Projects.RestClientTest);
         clientConfiguration.BaseNamespace = configuration.Projects.RestClientTest;
         clientConfiguration.ClientClassname = string.Empty;

         var unitTestClient = new UnitTestGenerator(clientConfiguration);
         unitTestClient.Generate(Log.Logger);
      }

      /// <summary>
      /// Generates all classes for the NetApi project
      /// </summary>
      private static void GenerateNetApiClasses(MetaData metadata, AjunaConfiguration configuration)
      {
         var generator = new NetApiGenerator(Log.Logger, configuration.Metadata.Runtime, new ProjectSettings(configuration.Projects.NetApi));
         generator.Generate(metadata);
      }

      /// <summary>
      /// Returns the directory path to .ajuna directory
      /// </summary>
      private static string ResolveConfigurationDirectory() => Path.Join(Environment.CurrentDirectory, ".ajuna");

      /// <summary>
      /// Returns the file path to .ajuna/ajuna-config.json
      /// </summary>
      private static string ResolveConfigurationFilePath() => Path.Join(ResolveConfigurationDirectory(), "ajuna-config.json");

      /// <summary>
      /// Returns the file path to .ajuna/metadata.json
      /// </summary>
      private static string ResolveMetadataFilePath() => Path.Join(ResolveConfigurationDirectory(), "metadata.json");

      private static string ResolveRestServiceAssembly(AjunaConfiguration configuration)
      {
         if (File.Exists(configuration.RestClientSettings.ServiceAssembly))
         {
            return configuration.RestClientSettings.ServiceAssembly;
         }

         string framework = $"net{Environment.Version.Major}.{Environment.Version.Minor}";
         string fp = ResolveServicePath(framework, "Release", configuration.Projects.RestService, configuration.RestClientSettings.ServiceAssembly);

         if (File.Exists(fp))
         {
            return fp;
         }
         else
         {
            Log.Information("The file path {path} does not exist.", fp);
         }

         // Check if Debug version exist (if Release isn't available)
         fp = ResolveServicePath(framework, "Debug", configuration.Projects.RestService, configuration.RestClientSettings.ServiceAssembly);
         if (File.Exists(fp))
         {
            return fp;
         }
         else
         {
            Log.Information("The file path {path} does not exist.", fp);
         }

         return string.Empty;
      }

      private static string ResolveServicePath(string framework, string configuration, string restServiceProject, string assembly)
      {
         return Path.Combine(
            ResolveConfigurationDirectory(),
            "..",
            restServiceProject,
            "bin",
            configuration,
            framework,
            assembly);
      }
   }
}

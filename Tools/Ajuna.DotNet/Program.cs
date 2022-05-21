﻿using Ajuna.DotNet.Generators;
using Ajuna.DotNet.Node;
using Ajuna.NetApi.Model.Meta;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using System;
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
                        if (! await UpgradeAjunaEnvironmentAsync(CancellationToken.None))
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
      private static async Task<bool> UpdateAjunaEnvironmentAsync(CancellationToken token) => await UpgradeOrUpdateAjunaEnvironmentAsync(token, fetchMetadata: false);

      /// <summary>
      /// Invoked with dotnet ajuna upgrade.
      /// This command first updates the metadata file and then generates all classes again.
      /// </summary>
      /// <returns>Returns true on success, otherwise false.</returns>
      private static async Task<bool> UpgradeAjunaEnvironmentAsync(CancellationToken token) => await UpgradeOrUpdateAjunaEnvironmentAsync(token, fetchMetadata: true);

      /// <summary>
      /// Handles the implementation to update or upgrade an ajuna environment
      /// Upgrading first fetches the metadata and stores it in .ajuna configuration directory.
      /// Then a normal update command is invoked to generate code for all given projects.
      /// </summary>
      /// <param name="token">Cancellation</param>
      /// <param name="fetchMetadata">Controls whether to fetch the metadata (upgrade) or not (update).</param>
      /// <returns>Returns true on success, otherwise false.</returns>
      private static async Task<bool> UpgradeOrUpdateAjunaEnvironmentAsync(CancellationToken token, bool fetchMetadata)
      {
         // Update an existing Ajuna project tree by reading the required configuration file
         // in the current directory in subdirectory .ajuna.
         var configurationFile = ResolveConfigurationFilePath();
         if (!File.Exists(configurationFile))
         {
            Log.Error("The configuration file {file} does not exist! Please create a configuration file so this toolchain can produce correct outputs. You can scaffold the configuration file by creating a new service project with `dotnet new ajuna-service`.");
            return false;
         }

         // Read ajuna-config.json
         var configuration = JsonConvert.DeserializeObject<AjunaConfiguration>(File.ReadAllText(configurationFile));
         if (configuration == null)
         {
            Log.Error("Could not parse the configuration file {file}! Please ensure that the configuration file format is correct.", configurationFile);
            return false;
         }

         Log.Information("Using RestService Project = {name}", configuration.Projects.RestService);
         Log.Information("Using NetApi Project = {name}", configuration.Projects.NetApi);
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

         var metadataFilePath = ResolveMetadataFilePath();
         Log.Information("Using Metadata = {metadataFilePath}", metadataFilePath);

         var metadata = GetMetadata.GetMetadataFromFile(Log.Logger, metadataFilePath);
         if (metadata == null)
            return false;

         GenerateRestServiceClasses(metadata, configuration);
         GenerateNetApiClasses(metadata, configuration);

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
         var metadata = await GetMetadata.GetMetadataFromNodeAsync(Log.Logger, websocket, token);
         if (metadata == null)
            throw new InvalidOperationException($"Could not query metadata from node {websocket}!");

         var metadataFilePath = ResolveMetadataFilePath();

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
         var generator = new RestServiceGenerator(Log.Logger, configuration.Metadata.Runtime, new ProjectSettings(configuration.Projects.RestService));
         generator.Generate(metadata);
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

   }
}

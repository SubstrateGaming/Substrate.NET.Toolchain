#nullable enable
using Ajuna.DotNet.Generators;
using Ajuna.NetApi;
using Ajuna.NetApi.Model.Meta;
using Ajuna.NetApi.Model.Types.Metadata;
using Serilog;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ajuna.DotNet
{
   internal class Generator
   {
      private readonly ILogger _logger;
      private readonly GeneratorSettings _settings;

      internal Generator(ILogger logger, GeneratorSettings settings)
      {
         _logger = logger;
         _settings = settings;
      }

      internal async Task GenerateAsync(CancellationToken cancellationToken)
      {
         if (!EnsureArgumentsValid())
            return;

         await GenerateServiceAsync(cancellationToken);
         await GenerateClientAsync(cancellationToken);
      }

      private async Task GenerateClientAsync(CancellationToken cancellationToken)
      {
         if (!_settings.WantClient)
            return;

         await Task.Delay(0);
      }

      private async Task GenerateServiceAsync(CancellationToken cancellationToken)
      {
         if (!_settings.WantService)
            return;

         MetaData? metadata = null;

         if (File.Exists(_settings.ServiceArgument))
         {
            // Service argument is a local file.
            // No need to download the metadata from substrate node.
            metadata = GetMetadataFromFile(_settings.ServiceArgument);
         }
         else
         {
            // Service argument is a connection string to a substrate node.
            // Must download metadata from node.
            metadata = await GetMetadataFromNodeAsync(_settings.ServiceArgument, cancellationToken);
         }

         if (metadata == null)
         {
            _logger.Error("Could not load required substrate metadata from node or file.");
            return;
         }

         _logger.Information("Loaded substrate metadata version {v} having origin {o} successfully.", metadata.Version, metadata.Origin);

         // Generate Rest Solution and build it in order to generate the RestService.dll
         var w1 = @"D:\tmp\w1";
         var w2 = @"D:\tmp\w2";

         GenerateRestServiceSolution(metadata, w1);

         // Generate Net Api Solution
         GenerateNetApiSolution(metadata, w2);



         await Task.Delay(0);
      }

      private bool EnsureArgumentsValid()
      {
         if (_settings.WantService && string.IsNullOrEmpty(_settings.ServiceArgument))
         {
            _logger.Error("Cannot use --service without argument to specifiy substrate metadata.");
            return false;
         }

         if (_settings.WantClient && string.IsNullOrEmpty(_settings.ClientArgument))
         {
            _logger.Error("Cannot use --client without argument to specifiy service layer binaries.");
            return false;
         }

         return true;
      }

      private MetaData? GetMetadataFromFile(string serviceArgument)
      {
         _logger.Information("Loading metadata from file {file}...", serviceArgument);

         try
         {
            var result = File.ReadAllText(serviceArgument);
            var runtimeMetadata = new RuntimeMetadata();

            runtimeMetadata.Create(result);
            return new MetaData(runtimeMetadata, string.Empty);
         }
         catch (Exception ex)
         {
            _logger.Error(ex, $"Error while loading metadata from file: {serviceArgument}.");
         }

         return null;
      }

      private async Task<MetaData?> GetMetadataFromNodeAsync(string serviceArgument, CancellationToken cancellationToken)
      {
         _logger.Information("Loading metadata from node {node}...", serviceArgument);

         try
         {
            using (var client = new SubstrateClient(new Uri(serviceArgument)))
            {
               await client.ConnectAsync(true, cancellationToken);
               await client.State.GetMetaDataAsync(cancellationToken);
               return client.MetaData;
            }
         }
         catch (Exception ex)
         {
            _logger.Error(ex, $"Error while loading metadata from node: {serviceArgument}.");
         }

         return null;
      }

      public void GenerateNetApiSolution(MetaData metadata, string workingDirectory)
      {
         var netApiGenerator = new NetApiSolutionGenerator(_settings.NodeRuntime, workingDirectory);
         netApiGenerator.Generate(metadata);
      }

      public void GenerateRestServiceSolution(MetaData metadata, string workingDirectory)
      {
         var restServiceGenerator = new RestServiceSolutionGenerator(_settings.NodeRuntime, workingDirectory);
         restServiceGenerator.Generate(metadata);
      }
   }
}

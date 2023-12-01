#nullable enable

using Serilog;
using Substrate.NetApi;
using Substrate.NetApi.Model.Extrinsics;
using Substrate.NetApi.Model.Meta;
using Substrate.NetApi.Model.Types.Metadata;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Substrate.DotNet.Service.Node
{
   internal static class GetMetadata
   {
      internal static MetaData? GetMetadataFromFile(ILogger logger, string serviceArgument)
      {
         logger.Information("Loading metadata from file {file}...", serviceArgument);

         try
         {
            return GetMetadataFromSerializedText(logger, File.ReadAllText(serviceArgument));
         }
         catch (Exception ex)
         {
            logger.Error(ex, $"Error while loading metadata from file: {serviceArgument}.");
         }

         return null;
      }

      internal static string GetRuntimeFromFile(ILogger logger, string serviceArgument)
      {
         logger.Information("Loading runtime from file {file}...", serviceArgument);

         try
         {
            return File.ReadAllText(serviceArgument).Replace("-", "_");
         }
         catch (Exception ex)
         {
            logger.Error(ex, $"Error while loading runtime from file: {serviceArgument}.");
         }

         return string.Empty;
      }

      internal static MetaData? GetMetadataFromSerializedText(ILogger logger, string serializedText)
      {
         try
         {
            var runtimeMetadata = new RuntimeMetadata();
            runtimeMetadata.Create(serializedText);
            return new MetaData(runtimeMetadata, string.Empty);
         }
         catch (Exception ex)
         {
            logger.Error(ex, $"Error while loading metadata from serialized text!");
         }

         return null;
      }

      internal static async Task<string?> GetMetadataFromNodeAsync(ILogger logger, string serviceArgument, CancellationToken cancellationToken)
      {
         logger.Information("Loading metadata from node {node}...", serviceArgument);

         try
         {
            using var client = new SubstrateClient(new Uri(serviceArgument), ChargeTransactionPayment.Default());
            await client.ConnectAsync(true, cancellationToken);
            return await client.State.GetMetaDataAsync(cancellationToken);
         }
         catch (Exception ex)
         {
            logger.Error(ex, $"Error while loading metadata from node: {serviceArgument}.");
         }

         return null;
      }

      internal static async Task<string?> GetRuntimeFromNodeAsync(ILogger logger, string serviceArgument, CancellationToken cancellationToken)
      {
         logger.Information("Loading runtime from node {node}...", serviceArgument);

         try
         {
            using var client = new SubstrateClient(new Uri(serviceArgument), ChargeTransactionPayment.Default());
            await client.ConnectAsync(true, cancellationToken);
            return $"{client.RuntimeVersion.SpecName}_runtime";
         }
         catch (Exception ex)
         {
            logger.Error(ex, $"Error while loading metadata from node: {serviceArgument}.");
         }

         return null;
      }
   }
}
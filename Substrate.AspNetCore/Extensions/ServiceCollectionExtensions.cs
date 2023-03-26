using Substrate.ServiceLayer;
using Substrate.ServiceLayer.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Substrate.AspNetCore.Extensions
{
   public static class ServiceCollectionExtension
   {
      public static IServiceCollection AddAjunaStorageService(this IServiceCollection services, SubstrateStorageServiceConfiguration configuration)
      {
         var substrateService = new SubstrateService();

         Task.Run(async () =>
         {

            // Initialize the storage service layer..
            // 1. A connection to the server is established
            // 2. Subscribe to all Storage Changes
            // 3. Gather all storage info from metadata and laod all Storage specific Delegates
            // 4. Start Processing Changes  
            await substrateService.InitializeAsync(configuration);

            // Save the reference for later use.
            SubstrateRuntime.SubstrateService = substrateService;

#pragma warning disable VSTHRD002
         }).Wait();
#pragma warning restore VSTHRD002

         if (SubstrateRuntime.SubstrateService == null)
         {
            throw new NotSupportedException("Could not initialize game service runtime. Please confirm that your configuration is correct.");
         }

         // Register storages for dependency injection..
         foreach (IStorage storage in configuration.Storages)
         {
            Type[] interfaceTypes = storage.GetType().GetInterfaces();
            if (interfaceTypes.Length > 0)
            {
               services.AddSingleton(interfaceTypes[0], storage);
            }
         }

         return services;
      }

      public static IServiceCollection AddSubstrateSubscriptionHandler(this IServiceCollection services)
      {
         services.AddTransient<SubscriptionManager>();
         services.AddSingleton<StorageSubscriptionHandler>();
         services.AddSingleton<SubscriptionMiddleware>();
         return services;
      }
   }
}
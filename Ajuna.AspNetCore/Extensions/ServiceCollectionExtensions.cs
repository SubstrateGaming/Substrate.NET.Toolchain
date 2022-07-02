using Ajuna.ServiceLayer;
using Ajuna.ServiceLayer.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Ajuna.AspNetCore.Extensions
{
   public static class ServiceCollectionExtension
   {
      public static IServiceCollection AddAjunaStorageService(this IServiceCollection services, AjunaStorageServiceConfiguration configuration)
      {
         var game = new AjunaSubstrateService();

         Task.Run(async () =>
         {

            // Initialize the storage service layer..
            await game.InitializeAsync(configuration);

            // Save the reference for later use.
            AjunaRuntime.GameService = game;

#pragma warning disable VSTHRD002
         }).Wait();
#pragma warning restore VSTHRD002

         if (AjunaRuntime.GameService == null)
         {
            throw new Exception("Could not initialize game service runtime. Please confirm that your configuration is correct.");
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

      public static IServiceCollection AddAjunaSubscriptionHandler(this IServiceCollection services)
      {
         services.AddTransient<SubscriptionManager>();
         services.AddSingleton<StorageSubscriptionHandler>();
         services.AddSingleton<SubscriptionMiddleware>();
         return services;
      }
   }
}
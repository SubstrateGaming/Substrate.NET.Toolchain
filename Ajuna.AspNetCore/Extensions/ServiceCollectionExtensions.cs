using Ajuna.ServiceLayer;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Ajuna.AspNetCore
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddAjunaStorageService(this IServiceCollection services, AjunaStorageServiceConfiguration configuration)
        {
            var game = new GameService();

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
            foreach (var storage in configuration.Storages)
            {
                Type[] interfaceTypes = storage.GetType().GetInterfaces();
                if (interfaceTypes.Length > 0)
                {
                    services.AddSingleton(interfaceTypes[0], storage);
                }
            }

            return services;
        }

        public static IServiceCollection AddAjunaSubscriptionHandler<TService>(this IServiceCollection services) where TService : class
        {
            services.AddTransient<SubscriptionManager>();
            services.AddSingleton<TService>();
            return services;
        }
    }
}
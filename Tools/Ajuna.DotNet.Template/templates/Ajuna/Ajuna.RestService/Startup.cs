using Ajuna.AspNetCore;
using Ajuna.AspNetCore.Extensions;
using Juce.RestService.Formatters;
using Ajuna.ServiceLayer;
using Ajuna.ServiceLayer.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.IO;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;

namespace Ajuna.RestService
{
   class AjunaOutputFormatterSetup : IConfigureOptions<MvcOptions>
   {
      void IConfigureOptions<MvcOptions>.Configure(MvcOptions options)
      {
         options.OutputFormatters.Insert(0, new AjunaOutputFormatter());
      }
   }

   public static class MvcBuilderExtensions
   {
      public static IMvcBuilder AddAjunaOutputFormatter(this IMvcBuilder builder)
      {
         builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, AjunaOutputFormatterSetup>());
         return builder;
      }
   }

   /// <summary>
   /// This class implements configuration and setting up services.
   /// </summary>
   public class Startup
   {
      private readonly CancellationTokenSource CTS = new CancellationTokenSource();

      private IStorageDataProvider _storageDataProvider;
      private readonly StorageSubscriptionChangeDelegate _storageChangeDelegate = new StorageSubscriptionChangeDelegate();

      /// <summary>
      /// >> Startup
      /// Constructs and initializes the Startup class.
      /// Stores the configuration object
      /// </summary>
      /// <param name="configuration">The service configuration.</param>
      public Startup(IConfiguration configuration)
      {
         Configuration = configuration;
      }

      /// <summary>
      /// Retreives the service configuration.
      /// </summary>
      public IConfiguration Configuration { get; }

      /// <summary>
      /// This method gets called by the runtime. Use this method to add services to the container. 
      /// </summary>
      /// <param name="services">Service collection to configure.</param>
      public void ConfigureServices(IServiceCollection services)
      {
         // Configure web sockets to allow clients to subscribe to storage changes.
         services.AddAjunaSubscriptionHandler<StorageSubscriptionHandler>();

         // Configure data provider
         // _storageDataProvider = new AjunaSubstrateDataProvider(Environment.GetEnvironmentVariable("AJUNA_WEBSOCKET_ENDPOINT") ?? "ws://127.0.0.1:9944");

         // TODO (svnscha): Remove hard coded path.
         _storageDataProvider = new AjunaMockupDataProvider(File.ReadAllText(@"D:\tmp\code\test\.ajuna\metadata.json"));

         // Configure storage services
         services.AddAjunaStorageService(new AjunaStorageServiceConfiguration()
         {
            CancellationToken = CTS.Token,
            DataProvider = _storageDataProvider,
            Storages = GetRuntimeStorages()
         });

         // Register data provider as singleton.
         services.AddSingleton(_storageDataProvider);

         services.AddRouting(options => { options.LowercaseQueryStrings = true; options.LowercaseUrls = true; });
         services.AddControllers(options => { })
            .AddAjunaOutputFormatter();

         services.AddSwaggerGen(c =>
         {
            c.CustomSchemaIds(type => type.ToString());
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Juce.RestService", Version = "v1" });
         });

      }

      private List<IStorage> GetRuntimeStorages()
      {
         return Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(anyType => anyType.IsClass && typeof(IStorage).IsAssignableFrom(anyType))
            .Select(storageType => (IStorage)Activator.CreateInstance(storageType, new object[] { _storageDataProvider, _storageChangeDelegate }))
            .ToList();
      }

      /// <summary>
      /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline. 
      /// </summary>
      /// <param name="app">Application builder</param>
      /// <param name="env">Service hosting environment</param>
      /// <param name="handler">Middleware to handle web socket subscriptions</param>
      public void Configure(IApplicationBuilder app, IWebHostEnvironment env, StorageSubscriptionHandler handler)
      {
         if (env.IsDevelopment())
         {
            app.UseDeveloperExceptionPage();
         }

         app.UseSwagger();
         app.UseSwaggerUI(
             c =>
             {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Juce.RestService v1");
             }
         );

         app.UseRouting();
         app.UseAuthorization();
         app.UseEndpoints(endpoints =>
         {
            endpoints.MapControllers();
         });

         app.UseWebSockets();

         // Set the singleton subscription handler to our storage change delegate
         // so it can process and broadcast changes to any connected client.
         _storageChangeDelegate.SetSubscriptionHandler(handler);

         // Accept the subscriptions from now on.
         // TODO (svnscha) Enable this again.
         // app.UseSubscription("/ws", handler);
      }
   }
}

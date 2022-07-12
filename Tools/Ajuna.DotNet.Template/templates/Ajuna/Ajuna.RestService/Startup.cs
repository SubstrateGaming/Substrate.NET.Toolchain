using Ajuna.AspNetCore;
using Ajuna.AspNetCore.Persistence;
using Ajuna.AspNetCore.Extensions;
using Ajuna.RestService.Formatters;
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

      // Delegate for adding local persistence for any Storage Changes
      // Changes are going to be saved in a CSV file. The default location of the CSV file will be in project root.
      // Alternatively, please set the fileDirectory parameter in the constructor below.
      private readonly StoragePersistenceChangeDelegate _storagePersistenceChangeDelegate = new StoragePersistenceChangeDelegate();
      
      // Set to true to activate persistence 
      private readonly bool _useLocalStoragePersistence = false;


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
      /// Retrieves the service configuration.
      /// </summary>
      public IConfiguration Configuration { get; }

      /// <summary>
      /// This method gets called by the runtime. Use this method to add services to the container. 
      /// </summary>
      /// <param name="services">Service collection to configure.</param>
      public void ConfigureServices(IServiceCollection services)
      {
         string useMockupProvider = Environment.GetEnvironmentVariable("AJUNA_USE_MOCKUP_PROVIDER") ?? "false";
         if (!string.IsNullOrEmpty(useMockupProvider) && useMockupProvider.Equals("true", StringComparison.InvariantCultureIgnoreCase))
         {
            // Configure mockup data provider
            _storageDataProvider = new AjunaMockupDataProvider(File.ReadAllText("..\\.ajuna\\metadata.txt"));
         }
         else
         {
            // Configure regular data provider
            _storageDataProvider = new AjunaSubstrateDataProvider(Environment.GetEnvironmentVariable("AJUNA_WEBSOCKET_ENDPOINT") ?? "ws://127.0.0.1:9944");
         }

         // Configure web sockets to allow clients to subscribe to storage changes.
         services.AddAjunaSubscriptionHandler();

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
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ajuna.RestService", Version = "v1" });
         });
      }

      private List<IStorage> GetRuntimeStorages()
      {
         var storageChangeDelegates = new List<IStorageChangeDelegate> {_storageChangeDelegate,};
          
         // If true, add local storage persistence
         if(_useLocalStoragePersistence)
            storageChangeDelegates.Add(_storagePersistenceChangeDelegate);
      
         return Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(anyType => anyType.IsClass && typeof(IStorage).IsAssignableFrom(anyType))
            .Select(storageType => (IStorage)Activator.CreateInstance(storageType, new object[] { _storageDataProvider, storageChangeDelegates }))
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ajuna.RestService v1");
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
         app.UseSubscription("/ws");
      }
   }
}

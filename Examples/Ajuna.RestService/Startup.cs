using Ajuna.AspNetCore;
using Ajuna.Infrastructure.Storages;
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
using System.IO;
using System.Threading;

namespace Ajuna.RestService
{
    public class Startup
    {
        private readonly CancellationTokenSource CTS = new CancellationTokenSource();

        private readonly StorageSubscriptionChangeDelegate _storageChangeDelegate = new StorageSubscriptionChangeDelegate();

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure web sockets to allow clients to subscribe to storage changes.
            services.AddGameServiceSubscriptions();
            
            services.AddGameService(new GameServiceConfiguration()
            {
                CancellationToken = CTS.Token,
                Endpoint = new Uri(Environment.GetEnvironmentVariable("AJUNA_WEBSOCKET_ENDPOINT") ?? "ws://127.0.0.1:9944"),
                Storages = new List<IStorage>()
                {
                    new AssetsStorage(_storageChangeDelegate),
                    // new BalancesStorage(_storageChangeDelegate),
                    //new ConnectFourStorage(_storageChangeDelegate),
                    //new ConnectFourMamaStorage(_storageChangeDelegate),
                    //new SchedulerStorage(),
                    //new SudoStorage(),
                    //new SystemStorage(_storageChangeDelegate),
                    //new LotteryStorage(_storageChangeDelegate)
                }
            });


            services.AddRouting(options => { options.LowercaseQueryStrings = true; options.LowercaseUrls = true; });
            services.AddControllers(options =>
            {
                options.OutputFormatters.Clear();
                options.OutputFormatters.Add(new AjunaOutputFormatter());
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "NodeTemplateCore.Infrastructure.RestService", Version = "v1" });

                // Set the comments path for the Swagger JSON and UI.
                var filePath = Path.Combine(AppContext.BaseDirectory, "Ajuna.RestService.xml");
                c.IncludeXmlComments(filePath);
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "NodeTemplateCore.Infrastructure.RestService v1");
                    //c.InjectStylesheet("/swagger-ui/custom.css");
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
            app.UseSubscription("/ws", handler);
        }
    }
}

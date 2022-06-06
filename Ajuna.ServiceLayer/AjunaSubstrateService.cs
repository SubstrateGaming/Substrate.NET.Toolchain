using Ajuna.ServiceLayer.Storage;
using Serilog;
using System.Threading.Tasks;

namespace Ajuna.ServiceLayer
{
   public class AjunaSubstrateService
   {
      private readonly AjunaSubstrateStorage GameStorage = new AjunaSubstrateStorage();

      public async Task InitializeAsync(AjunaStorageServiceConfiguration configuration)
      {
         Log.Information("initialize Ajuna substrate service");

         //
         // Initialize substrate client API
         //
         await configuration.DataProvider.ConnectAsync(configuration.CancellationToken);

         //
         // Initialize storage systems
         // Start by subscribing to any storage change and then start loading
         // all storages that this service is interested in.
         //
         // While we are loading storages any storage subscription notification will
         // wait to be processed until the initialization is complete.
         await configuration.DataProvider.SubscribeStorageAsync(GameStorage.OnStorageUpdate);

         // Load storages we are interested in.
         await GameStorage.InitializeAsync(configuration.DataProvider, configuration.Storages);

         // Start processing subscriptions.
         GameStorage.StartProcessingChanges();
      }

      public IStorage GetStorage<T>() => GameStorage.GetStorage<T>();
   }
}

using Ajuna.ServiceLayer.Storage;
using Serilog;
using System.Threading.Tasks;

namespace Ajuna.ServiceLayer
{
   public class AjunaSubstrateService
   {
      private readonly AjunaSubstrateStorage _ajunaSubstrateStorage = new AjunaSubstrateStorage();

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
         // wait to be processed after the initialization is complete.
         await configuration.DataProvider.SubscribeStorageAsync(_ajunaSubstrateStorage.OnStorageUpdate);

         // Load storages we are interested in.
         await _ajunaSubstrateStorage.InitializeAsync(configuration.DataProvider, configuration.Storages);

         // Start processing subscriptions.
         _ajunaSubstrateStorage.StartProcessingChanges();
      }

      public IStorage GetStorage<T>() => _ajunaSubstrateStorage.GetStorage<T>();
   }
}

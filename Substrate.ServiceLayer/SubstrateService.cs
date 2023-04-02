using Substrate.ServiceLayer.Storage;
using Serilog;
using System.Threading.Tasks;

namespace Substrate.ServiceLayer
{
   public class SubstrateService
   {
      private readonly SubstrateStorage _substrateStorage = new SubstrateStorage();

      /// <summary>
      /// 1. A connection to the server is established
      /// 2. Subscribe to all Storage Changes
      /// 3. Gather all storage info from metadata and laod all Storage specific Delegates
      /// 4. Start Processing Changes  
      /// </summary>
      /// <param name="configuration"></param>
      public async Task InitializeAsync(SubstrateStorageServiceConfiguration configuration)
      {
         Log.Information("initialize substrate service");

         // Initialize substrate client API
         await configuration.DataProvider.ConnectAsync(configuration.CancellationToken);

         // Initialize storage systems
         // Start by subscribing to any storage change and then start loading
         // all storages that this service is interested in.

         // While we are loading storages any storage subscription notification will
         // wait to be processed after the initialization is complete.

         //TODO: this call needs to be fixed for chains that don't allow subscribe to all storages
         //await configuration.DataProvider.SubscribeStorageAsync(_substrateStorage.OnStorageUpdate);
         await _substrateStorage.SubscribeAsync(configuration.DataProvider, _substrateStorage.OnStorageUpdate);

         // Load storages we are interested in and register all Storage specific Delegates
         await _substrateStorage.InitializeAsync(configuration.DataProvider, configuration.Storages, configuration.IsLazyLoadingEnabled);

         // Start processing subscriptions.
         _substrateStorage.StartProcessingChanges();
      }

      public IStorage GetStorage<T>() => _substrateStorage.GetStorage<T>();
   }
}

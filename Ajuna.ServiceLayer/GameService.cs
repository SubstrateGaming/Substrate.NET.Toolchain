using Ajuna.NetApi;
using Ajuna.ServiceLayer.Storage;
using Serilog;
using System.Threading.Tasks;

namespace Ajuna.ServiceLayer
{
    public class GameService
    {
        private SubstrateClient Client;

        private readonly GameStorage GameStorage = new GameStorage();

        public async Task InitializeAsync(AjunaStorageServiceConfiguration configuration)
        {
            Log.Information("initialize GameService");

            //
            // Initialize substrate client API
            //
            Log.Information("substrate client connecting to {uri}", configuration.Endpoint);

            Client = new SubstrateClient(configuration.Endpoint);
            await Client.ConnectAsync(configuration.CancellationToken);

            Log.Information("substrate client connected");

            //
            // Initialize storage systems
            // Start by subscribing to any storage change and then start loading
            // all storages that this service is interested in.
            //
            // While we are loading storages any storage subscription notification will
            // wait to be processed until the initialization is complete.

            //var keys = Utils.Bytes2HexString(RequestGenerator.GetStorageKeyBytesHash("System","Account"), Utils.HexStringFormat.Prefixed);
            //var keys = Utils.Bytes2HexString(RequestGenerator.GetStorageKeyBytesHash("Balances", "Account"), Utils.HexStringFormat.Prefixed);
            //await Client.State.SubscribeStorageAsync(new JArray { keys }, GameStorage.OnStorageUpdate);
            await Client.State.SubscribeStorageAsync(null, GameStorage.OnStorageUpdate);

            // Load storages we are interested in.
            await GameStorage.InitializeAsync(Client, configuration.Storages);

            // Start processing subscriptions.
            GameStorage.StartProcessingChanges();
        }

        public IStorage GetStorage<T>() => GameStorage.GetStorage<T>();
    }
}

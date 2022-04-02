using Ajuna.RestService.Subscription.Model;
using Ajuna.ServiceLayer.Storage;

namespace Ajuna.RestService
{
    internal class StorageSubscriptionChangeDelegate : IStorageChangeDelegate
    {
        private StorageSubscriptionHandler _handler;

        public void OnCreate(string identifier, string key, string data)
        {
            _handler?.BroadcastChange(identifier, key, data, StorageSubscriptionChangeType.Create);
        }

        public void OnDelete(string identifier, string key, string data)
        {
            _handler?.BroadcastChange(identifier, key, data, StorageSubscriptionChangeType.Delete);
        }

        public void OnUpdate(string identifier, string key, string data)
        {
            _handler?.BroadcastChange(identifier, key, data, StorageSubscriptionChangeType.Update);
        }

        internal void SetSubscriptionHandler(StorageSubscriptionHandler handler)
        {
            _handler = handler;
        }
    }
}
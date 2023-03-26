using Substrate.ServiceLayer.Model;
using Substrate.ServiceLayer.Storage;

namespace Substrate.AspNetCore
{
   public class StorageSubscriptionChangeDelegate : IStorageChangeDelegate
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

      public void SetSubscriptionHandler(StorageSubscriptionHandler handler)
      {
         _handler = handler;
      }
   }
}
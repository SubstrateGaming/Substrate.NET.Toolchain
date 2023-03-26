using Substrate.NetApi;
using Substrate.NetApi.Model.Extrinsics;
using Substrate.NetApi.Model.Meta;
using Substrate.NetApi.Model.Rpc;
using Substrate.NetApi.Model.Types;
using Substrate.ServiceLayer.Extensions;
using Substrate.ServiceLayer.Storage;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Substrate.ServiceLayer
{
   public class AjunaSubstrateDataProvider : IStorageDataProvider
   {
      public SubstrateClient Client { get; private set; }

      public AjunaSubstrateDataProvider(string endpoint)
      {
         Client = new SubstrateClient(new Uri(endpoint), ChargeTransactionPayment.Default());
      }

      public async Task<T> GetStorageAsync<T>(string module, string storageName) where T : IType, new()
      {
         return await Client.GetStorageAsync<T>(module, storageName);
      }

      public async Task<Dictionary<string, T>> GetStorageDictAsync<T>(string module, string storageName) where T : IType, new()
      {
         return await Client.GetStorageDictAsync<T>(module, storageName);
      }

      public MetaData GetMetadata()
      {
         return Client.MetaData;
      }

      public async Task ConnectAsync(CancellationToken cancellationToken)
      {
         await Client.ConnectAsync(cancellationToken);
      }

      public async Task SubscribeStorageAsync(Action<string, StorageChangeSet> onStorageUpdate)
      {
         await Client.State.SubscribeStorageAsync(null, onStorageUpdate);
      }

      public void BroadcastLocalStorageChange(string id, StorageChangeSet changeSet)
      {
         // Do not implement this in this provider.
         // This is only useful for mockup providers.
         throw new NotImplementedException();
      }
   }
}

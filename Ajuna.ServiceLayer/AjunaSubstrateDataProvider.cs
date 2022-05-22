using Ajuna.NetApi;
using Ajuna.NetApi.Model.Meta;
using Ajuna.NetApi.Model.Rpc;
using Ajuna.NetApi.Model.Types;
using Ajuna.ServiceLayer.Extensions;
using Ajuna.ServiceLayer.Storage;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ajuna.ServiceLayer
{
   public class AjunaSubstrateDataProvider : IStorageDataProvider
   {
      public SubstrateClient Client { get; private set; }

      public AjunaSubstrateDataProvider(string endpoint)
      {
         Client = new SubstrateClient(new Uri(endpoint));
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
   }
}

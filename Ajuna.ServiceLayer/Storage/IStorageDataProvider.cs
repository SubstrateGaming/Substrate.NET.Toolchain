using Ajuna.NetApi.Model.Meta;
using Ajuna.NetApi.Model.Rpc;
using Ajuna.NetApi.Model.Types;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ajuna.ServiceLayer.Storage
{
   public interface IStorageDataProvider
   {
      Task<Dictionary<string, T>> GetStorageDictAsync<T>(string module, string storageName) where T : IType, new();
      Task<T> GetStorageAsync<T>(string module, string storageName) where T : IType, new();
      MetaData GetMetadata();
      Task ConnectAsync(CancellationToken cancellationToken);
      Task SubscribeStorageAsync(Action<string, StorageChangeSet> onStorageUpdate);
      void BroadcastLocalStorageChange(string id, StorageChangeSet changeSet);
   }
}

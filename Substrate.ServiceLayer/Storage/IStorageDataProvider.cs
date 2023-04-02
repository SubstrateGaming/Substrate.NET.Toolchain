using Newtonsoft.Json.Linq;
using Substrate.NetApi.Model.Meta;
using Substrate.NetApi.Model.Rpc;
using Substrate.NetApi.Model.Types;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Substrate.ServiceLayer.Storage
{
   /// <summary>
   /// Provides data fetching and subscription functionality for the node
   /// </summary>
   public interface IStorageDataProvider
   {
      Task<Dictionary<string, T>> GetStorageDictAsync<T>(string module, string storageName) where T : IType, new();
      Task<T> GetStorageAsync<T>(string module, string storageName) where T : IType, new();
      MetaData GetMetadata();
      Task ConnectAsync(CancellationToken cancellationToken);
      
      /// <summary>
      /// Subscribes to all storage changes
      /// </summary>
      /// <param name="onStorageUpdate">Delegate to be executed on every storage change</param>
      /// <returns></returns>
      Task SubscribeStorageAsync(JArray keys, Action<string, StorageChangeSet> onStorageUpdate);
      void BroadcastLocalStorageChange(string id, StorageChangeSet changeSet);
   }
}

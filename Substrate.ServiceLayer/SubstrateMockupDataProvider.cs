using Newtonsoft.Json.Linq;
using Substrate.NetApi.Model.Meta;
using Substrate.NetApi.Model.Rpc;
using Substrate.NetApi.Model.Types;
using Substrate.NetApi.Model.Types.Metadata;
using Substrate.NetApi.Model.Types.Metadata.V14;
using Substrate.ServiceLayer.Storage;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Substrate.ServiceLayer
{
   public class SubstrateMockupDataProvider : IStorageDataProvider
   {
      private readonly MetaData _metaData;
      private Action<string, StorageChangeSet> _storageUpdate;

      public SubstrateMockupDataProvider(string metadata)
      {
         var runtimeMetadata = new RuntimeMetadata<RuntimeMetadataV14>();
         runtimeMetadata.Create(metadata);

         _metaData = new MetaData(runtimeMetadata, string.Empty);
      }

      public Task ConnectAsync(CancellationToken cancellationToken)
      {
         return Task.FromResult(0);
      }

      public MetaData GetMetadata()
      {
         return _metaData;
      }

      public Task<T> GetStorageAsync<T>(string module, string storageName) where T : IType, new()
      {
         return Task.FromResult(new T());
      }

      public Task<Dictionary<string, T>> GetStorageDictAsync<T>(string module, string storageName) where T : IType, new()
      {
         return Task.FromResult(new Dictionary<string, T>());
      }

      public Task SubscribeStorageAsync(JArray keys, Action<string, StorageChangeSet> onStorageUpdate)
      {
         _storageUpdate = onStorageUpdate;
         return Task.FromResult(0);
      }

      public void BroadcastLocalStorageChange(string id, StorageChangeSet changeSet)
      {
         _storageUpdate?.Invoke(id, changeSet);
      }
   }
}

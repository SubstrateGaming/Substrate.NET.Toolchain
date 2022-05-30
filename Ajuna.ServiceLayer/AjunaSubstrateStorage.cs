using Ajuna.NetApi;
using Ajuna.NetApi.Model.Meta;
using Ajuna.NetApi.Model.Rpc;
using Ajuna.ServiceLayer.Attributes;
using Ajuna.ServiceLayer.Storage;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Ajuna.ServiceLayer
{
   internal class AjunaSubstrateStorage
   {
      private readonly ManualResetEvent StorageStartProcessingEvent = new ManualResetEvent(false);
      private readonly object Lock = new object();

      private readonly Dictionary<string, ItemInfo> StorageModuleItemInfos = new Dictionary<string, ItemInfo>();

      private readonly Dictionary<string, Tuple<object, MethodInfo>> StorageChangeListener = new Dictionary<string, Tuple<object, MethodInfo>>();

      private List<IStorage> Storages = new List<IStorage>();

      struct ItemInfo
      {
         public string ModuleName { get; internal set; }
         public string StorageName { get; internal set; }
      }

      internal IStorage GetStorage<T>()
      {
         foreach (IStorage storage in Storages)
         {
            if (storage.GetType().GetInterfaces().Contains(typeof(T)))
            {
               return storage;
            }
         }

         throw new KeyNotFoundException($"Could not find storage {typeof(T).Name} in storage list.");
      }

      internal async Task InitializeAsync(IStorageDataProvider dataProvider, List<IStorage> storages)
      {
         Storages = storages;

         InitializeMetadataDisplayNames(dataProvider.GetMetadata());
         InitializeStorageChangeListener();

         foreach (IStorage storage in Storages)
         {
            await storage.InitializeAsync(dataProvider);
         }
      }

      private void InitializeStorageChangeListener()
      {
         foreach (IStorage storage in Storages)
         {
            foreach (MethodInfo method in storage.GetType().GetMethods())
            {
               object[] attributes = method.GetCustomAttributes(typeof(StorageChangeAttribute), true);
               foreach (object attribute in attributes)
               {
                  var listenerMethod = attribute as StorageChangeAttribute;
                  StorageChangeListener.Add(listenerMethod.Key, new Tuple<object, MethodInfo>(storage, method));
               }
            }
         }
      }

      private void InitializeMetadataDisplayNames(MetaData metadata)
      {
         foreach (PalletModule palletModule in metadata.NodeMetadata.Modules.Values)
         {
            string moduleName = palletModule.Name;

            if (palletModule.Storage == null || palletModule.Storage.Entries == null)
            {
               continue;
            }

            foreach (Entry storage in palletModule.Storage.Entries)
            {
               var itemInfo = new ItemInfo
               {
                  ModuleName = moduleName,
                  StorageName = storage.Name
               };

               Log.Debug("loading [{module}, {name}]: {state}", itemInfo.ModuleName, itemInfo.StorageName, "Ok");

               string key = Utils.Bytes2HexString(
                   RequestGenerator.GetStorageKeyBytesHash(itemInfo.ModuleName, itemInfo.StorageName),
                   Utils.HexStringFormat.Prefixed)
                   .ToLower();

               if (!StorageModuleItemInfos.ContainsKey(key))
               {
                  StorageModuleItemInfos.Add(key, itemInfo);
               }
            }
         }

         Log.Information("loaded storage metadata modules {count}", StorageModuleItemInfos.Count);
      }

      [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "May be used later.")]
      internal void OnStorageUpdate(string id, StorageChangeSet changes)
      {
         lock (Lock)
         {
            // Block the current thread until we received the initialize signal.
            // This function returns immediately once the signal was set at least once.
            StorageStartProcessingEvent.WaitOne();

            // Process the changes.
            foreach (string[] change in changes.Changes)
            {
               // The key starts with 0x prefix.
               string key = change[0].ToLower();

               if (key.Length < 66)
               {
                  Log.Debug($"Key of {key.Length} is to small for storage access!");
                  continue;
               }

               string indexedKey = $"0x{key.Substring(2, 32)}{key.Substring(34, 32)}";

               if (!StorageModuleItemInfos.TryGetValue(indexedKey, out ItemInfo itemInfo))
               {
                  Log.Debug($"Unable to find a storage module with key={indexedKey}!");
                  continue;
               }

               Log.Debug("OnStorageUpdate {module}.{storage}!", itemInfo.ModuleName, itemInfo.StorageName);

               if (key.Length == 66)
               {
                  ProcessStorageChange(itemInfo, Array.Empty<string>(), change[1]);
               }
               else
               {
                  ProcessStorageChange(itemInfo, new string[] { key }, change[1]);
               }
            }
         }
      }

      internal void StartProcessingChanges()
      {
         StorageStartProcessingEvent.Set();
      }

      private void ProcessStorageChange(ItemInfo itemInfo, string[] storageItemKeys, string data)
      {
         string key = $"{itemInfo.ModuleName}.{itemInfo.StorageName}";

         if (StorageChangeListener.ContainsKey(key))
         {
            Tuple<object, MethodInfo> listener = StorageChangeListener[key];

            string[] parameters = new string[storageItemKeys.Length + 1];
            parameters[parameters.Length - 1] = data;
            switch (storageItemKeys.Length)
            {
               case 0:
                  break;
               case 1:
                  parameters[0] = storageItemKeys[0];
                  break;
               default:
                  throw new NotImplementedException("Only one storage key accessed for generic service layer!");
            }

            listener.Item2.Invoke(listener.Item1, parameters);
         }
      }
   }
}

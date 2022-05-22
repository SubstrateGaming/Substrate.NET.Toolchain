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

      private readonly Dictionary<string, string> StorageModuleNames = new Dictionary<string, string>();
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

      internal async Task InitializeAsync(SubstrateClient client, List<IStorage> storages)
      {
         Storages = storages;

         InitializeMetadataDisplayNames(client);
         InitializeStorageChangeListener();

         foreach (IStorage storage in Storages)
         {
            await storage.InitializeAsync(client);
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

      private void InitializeMetadataDisplayNames(SubstrateClient client)
      {
         foreach (PalletModule palletModule in client.MetaData.NodeMetadata.Modules.Values)
         {
            string moduleName = palletModule.Name;

            // skip pallets with out storage
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

               //Log.Debug("ItemInfo[{module}, {name}]: {state}", itemInfo.ModuleName, itemInfo.StorageName, "Ok");

               string key = Utils.Bytes2HexString(
                   RequestGenerator.GetStorageKeyBytesHash(itemInfo.ModuleName, itemInfo.StorageName),
                   Utils.HexStringFormat.Prefixed)
                   .ToLower();

               string moduleNameHash = $"0x{key.Substring(2, 32)}";
               string storageItemNameHash = $"0x{key.Substring(34, 32)}";

               if (!StorageModuleNames.ContainsKey(moduleNameHash))
               {
                  StorageModuleNames.Add(moduleNameHash, itemInfo.ModuleName);
               }

               if (!StorageModuleItemInfos.ContainsKey(storageItemNameHash))
               {
                  StorageModuleItemInfos.Add(storageItemNameHash, itemInfo);
               }
            }
         }

         Log.Information("loaded storage metadata module names {count}", StorageModuleNames.Count);

         Log.Information("loaded storage metadata module item names {count}", StorageModuleItemInfos.Count);
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

               // [0x][Hash128(ModuleName)][Hash128(StorageName)]
               string moduleNameHash = $"0x{key.Substring(2, 32)}";

               string storageItemNameHash = $"0x{key.Substring(34, 32)}";
               if (!StorageModuleNames.TryGetValue(moduleNameHash, out _))
               {
                  Log.Debug($"Unable to find a module with moduleNameHash {moduleNameHash}!");
                  continue;
               }

               if (!StorageModuleItemInfos.TryGetValue(storageItemNameHash, out ItemInfo itemInfo))
               {
                  Log.Debug($"Unable to find a storage with storageItemNameHash {storageItemNameHash}!");
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

﻿using Newtonsoft.Json.Linq;
using Serilog;
using Substrate.NetApi;
using Substrate.NetApi.Model.Meta;
using Substrate.NetApi.Model.Rpc;
using Substrate.ServiceLayer.Attributes;
using Substrate.ServiceLayer.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Substrate.ServiceLayer
{
   internal class SubstrateStorage
   {
      private readonly ManualResetEvent StorageStartProcessingEvent = new ManualResetEvent(false);
      private readonly object Lock = new object();

      // Dictionary holding the info for each Storage. The key here is the Storage Key in its HEX encoded form.
      private readonly Dictionary<string, ItemInfo> StorageModuleItemInfos = new Dictionary<string, ItemInfo>();

      private readonly Dictionary<string, Tuple<object, MethodInfo>> StorageChangeDelegates = new Dictionary<string, Tuple<object, MethodInfo>>();

      private List<IStorage> Storages = new List<IStorage>();

      private struct ItemInfo
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

      /// <summary>
      ///
      /// </summary>
      /// <param name="dataProvider"></param>
      /// <param name="onStorageUpdate"></param>
      /// <returns></returns>
      internal async Task SubscribeAsync(IStorageDataProvider dataProvider, Action<string, StorageChangeSet> onStorageUpdate)
      {
         // Gather all Storage Info from the Metadata
         InitializeMetadataDisplayNames(dataProvider.GetMetadata());

         var array = new JArray();
         foreach (ItemInfo itemInfo in StorageModuleItemInfos.Values)
         {
            string key = Utils.Bytes2HexString(RequestGenerator.GetStorageKeyBytesHash(itemInfo.ModuleName, itemInfo.StorageName)).ToLower();
            Log.Debug("Adding {key} for {module}.{method}", key, itemInfo.ModuleName, itemInfo.StorageName);
            array.Add(key);
         }
         await dataProvider.SubscribeStorageAsync(array, onStorageUpdate);
      }

      /// <summary>
      /// Gather all storage info from metadata and laod all Storage specific Delegates
      /// </summary>
      /// <param name="dataProvider"></param>
      /// <param name="storages"></param>
      /// /// <param name="initialzeStorages"></param>
      internal async Task InitializeAsync(IStorageDataProvider dataProvider, List<IStorage> storages, bool isLazyLoadingEnabled)
      {
         Storages = storages;

         // Register all Storage specific Delegates
         InitializeStorageChangeListener();

          if (!isLazyLoadingEnabled)
         {
            // Initializes Storages fetching all their initial Values
            foreach (IStorage storage in Storages)
            {
               await storage.InitializeAsync(dataProvider);
            }
         }
      }

      /// <summary>
      /// Registers the Listener for all Storages
      /// </summary>
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
                  StorageChangeDelegates.Add(listenerMethod.Key, new Tuple<object, MethodInfo>(storage, method));
               }
            }
         }
      }

      /// <summary>
      /// Gathers all Storage Info from the Metadata
      /// </summary>
      /// <param name="metadata"></param>
      private void InitializeMetadataDisplayNames(MetaData metadata)
      {
         // Iterate through all pallets
         foreach (PalletModule palletModule in metadata.NodeMetadata.Modules.Values)
         {
            string moduleName = palletModule.Name;

            if (palletModule.Storage == null || palletModule.Storage.Entries == null)
            {
               continue;
            }

            // For each storage that you find, get the Pallet/Module and Storage Name and add them to the
            // Dictionary that has the Storage Hex key as a key
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

      /// <summary>
      /// Handles the incoming Storage Change
      /// </summary>
      /// <param name="id"></param>
      /// <param name="changes"></param>
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

               Log.Debug("OnStorageUpdate {module}.{storage}!)", itemInfo.ModuleName, itemInfo.StorageName);

               //if (key.Length == 66)
               //{
               //   ProcessStorageChange(itemInfo, Array.Empty<string>(), change[1]);
               //}
               //else
               //{
               //   ProcessStorageChange(itemInfo, new string[] { key }, change[1]);
               //}

               ProcessStorageChange(itemInfo, new string[] { key }, change[1]);
            }
         }
      }

      internal void StartProcessingChanges()
      {
         StorageStartProcessingEvent.Set();
      }

      /// <summary>
      /// Looks in the StorageChangeListeners to see
      /// if a listener is already registered for the incoming change and triggers it.
      /// </summary>
      /// <param name="itemInfo"></param>
      /// <param name="storageItemKeys"></param>
      /// <param name="data"></param>
      /// <exception cref="NotImplementedException"></exception>
      private void ProcessStorageChange(ItemInfo itemInfo, string[] storageItemKeys, string data)
      {
         string key = $"{itemInfo.ModuleName}.{itemInfo.StorageName}";

         if (StorageChangeDelegates.ContainsKey(key))
         {
            Tuple<object, MethodInfo> listener = StorageChangeDelegates[key];

            int countParameters = listener.Item2.GetParameters().Length;

            string[] parameters = new string[countParameters];
            parameters[parameters.Length - 1] = data;
            switch (parameters.Length)
            {
               case 1:
                  // no key
                  break;

               case 2:
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
﻿using Newtonsoft.Json.Linq;
using Substrate.NetApi;
using Substrate.NetApi.Model.Rpc;
using Substrate.NetApi.Model.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Substrate.ServiceLayer.Extensions
{
   internal static class SubstrateClientExtensions
   {
      internal static async Task<Dictionary<string, T>> GetStorageDictAsync<T>(this SubstrateClient Client, string module, string storageName) where T : IType, new()
      {
         return await Client.GetStorageDictV2Async<T>(module, storageName);
      }

      internal static async Task<Dictionary<string, T>> GetStorageDictV1Async<T>(this SubstrateClient Client, string module, string storageName) where T : IType, new()
      {
         byte[] keyBytes = RequestGenerator.GetStorageKeyBytesHash(module, storageName);
         string keyString = Utils.Bytes2HexString(RequestGenerator.GetStorageKeyBytesHash(module, storageName)).ToLower();
         JArray keys = await Client.State.GetPairsAsync(keyBytes);
         var result = new Dictionary<string, T>();
         foreach (JToken child in keys.Children())
         {
            string key = child[0].ToString().Replace(keyString, string.Empty);
            var value = new T();
            value.Create(child[1].ToString());
            result[key] = value;
         }
         return result;
      }

      internal static async Task<Dictionary<string, T>> GetStorageDictV2Async<T>(this SubstrateClient Client, string module, string storageName) where T : IType, new()
      {
         // We will first get all Storage Keys and the their values for the fetched keys
         var result = new Dictionary<string, T>();

         // Get the keys first
         List<string> storageKeys = await GetStorageKeysAsync(Client, module, storageName);
         var cts = new CancellationTokenSource();

         IEnumerable<IEnumerable<string>> storageKeyBatches = storageKeys.BuildChunksWithLinqAndYield(500);

         foreach (IEnumerable<string> storageKeyBatch in storageKeyBatches)
         {
            var keyList = storageKeyBatch.Select(p => Utils.HexToByteArray(p.ToString())).ToList();
            IEnumerable<StorageChangeSet> storageChangeSets = await Client.State.GetQueryStorageAtAsync(keyList, (string)null, cts.Token);
            if (storageChangeSets != null)
            {
               foreach (string[] storageChangeSet in storageChangeSets.First().Changes)
               {
                  var t = default(T);
                  if (storageChangeSet[1] != null)
                  {
                     t = new T();
                     t.Create(storageChangeSet[1]);
                  }
                  result.Add(storageChangeSet[0], t);
               }
            }
         }

         return result;
      }

      internal static async Task<List<string>> GetStorageKeysAsync(this SubstrateClient Client, string module, string storageName, uint numberOfResultsPerPage = 100)
      {
         var keysList = new List<string>();

         byte[] keyBytes = RequestGenerator.GetStorageKeyBytesHash(module, storageName);
         string keyString = Utils.Bytes2HexString(keyBytes).ToLower();

         bool continueFetchingKeys = true;
         byte[] startKey = null;

         // Keep getting the keys until no results are left to fetch
         while (continueFetchingKeys)
         {
            JArray results = await Client.State.GetKeysPagedAsync(keyBytes, numberOfResultsPerPage, startKey);

            if (results.Count == 0)
            {
               return keysList;
            }

            if (results.Count < numberOfResultsPerPage)
            {
               continueFetchingKeys = false;
            }
            else
            {
               JToken lastKey = results.Children().Last();
               startKey = Utils.HexToByteArray(lastKey.ToString());
            }

            foreach (JToken child in results.Children())
            {
               string key = child.ToString().Replace(keyString, string.Empty);
               keysList.Add(key);
            }
         }

         return keysList;
      }

      internal static async Task<T> GetStorageAsync<T>(this SubstrateClient Client, string module, string storageName) where T : IType, new()
      {
         byte[] keyBytes = RequestGenerator.GetStorageKeyBytesHash(module, storageName);
         object resultStr = await Client.State.GetStorageAsync(keyBytes);
         var value = new T();
         if (resultStr != null)
         {
            value.Create(resultStr.ToString());
         }
         return value;
      }

      internal static IEnumerable<IEnumerable<T>> BuildChunksWithLinqAndYield<T>(this IEnumerable<T> fullList, int batchSize)
      {
         int total = 0;
         while (total < fullList.Count())
         {
            yield return fullList.Skip(total).Take(batchSize);
            total += batchSize;
         }
      }
   }
}
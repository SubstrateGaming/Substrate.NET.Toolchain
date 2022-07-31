using Ajuna.NetApi;
using Ajuna.NetApi.Model.Types;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ajuna.ServiceLayer.Extensions
{
   internal static class SubstrateClientExtensions
   {
      internal static async Task<Dictionary<string, T>> GetStorageDictAsync<T>(this SubstrateClient Client,
         string module, string storageName) where T : IType, new()
      {
         // We will first get all Storage Keys and the their values for the fetched keys

         var result = new Dictionary<string, T>();

         // Get the keys first
         List<string> storageKeys = await GetStorageKeysAsync(Client, module, storageName);
         var cts = new CancellationTokenSource();

         // Fetch storage for every key
         foreach (string key in storageKeys)
         {
            T value = await Client.GetStorageAsync<T>(key, cts.Token);
            result[key] = value;
         }

         return result;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="Client"></param>
      /// <param name="module"></param>
      /// <param name="storageName"></param>
      /// <param name="numberOfResultsPerPage"></param>
      /// <returns></returns>
      internal static async Task<List<string>> GetStorageKeysAsync(this SubstrateClient Client, string module,
         string storageName, int numberOfResultsPerPage = 10)
      {
         var keysList = new List<string>();

         byte[] keyBytes = RequestGenerator.GetStorageKeyBytesHash(module, storageName);
         string keyString = Utils.Bytes2HexString(keyBytes).ToLower();

         bool continueFetchingKeys = true;
         byte[] startKey = new byte[] { };

         // Keep getting the keys until no results are left to fetch
         while (continueFetchingKeys)
         {
            JArray results =
               await Client.State.GetKeysPagedAsync(keyBytes, UInt32.Parse(numberOfResultsPerPage.ToString()),
                  startKey);

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

      internal static async Task<T> GetStorageAsync<T>(this SubstrateClient Client, string module, string storageName)
         where T : IType, new()
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
   }
}
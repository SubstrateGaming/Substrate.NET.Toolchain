using Ajuna.NetApi;
using Ajuna.NetApi.Model.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ajuna.ServiceLayer.Extensions
{
   internal static class SubstrateClientExtensions
   {
      internal static async Task<Dictionary<string, T>> GetStorageDictAsync<T>(this SubstrateClient Client, string module, string storageName) where T : IType, new()
      {
         var keyBytes = RequestGenerator.GetStorageKeyBytesHash(module, storageName);
         var keyString = Utils.Bytes2HexString(RequestGenerator.GetStorageKeyBytesHash(module, storageName)).ToLower();
         var keys = await Client.State.GetPairsAsync(keyBytes);
         var result = new Dictionary<string, T>();
         foreach (var child in keys.Children())
         {
            var key = child[0].ToString().Replace(keyString, string.Empty);
            var value = new T();
            value.Create(child[1].ToString());
            result[key] = value;
         }
         return result;
      }

      internal static async Task<T> GetStorageAsync<T>(this SubstrateClient Client, string module, string storageName) where T : IType, new()
      {
         var keyBytes = RequestGenerator.GetStorageKeyBytesHash(module, storageName);
         var keyString = Utils.Bytes2HexString(RequestGenerator.GetStorageKeyBytesHash(module, storageName)).ToLower();
         var resultStr = await Client.State.GetStorageAsync(keyBytes);
         var value = new T();
         if (resultStr != null)
         {
            value.Create(resultStr.ToString());
         }
         return value;
      }
   }
}

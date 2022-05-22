using Ajuna.NetApi;
using Ajuna.NetApi.Model.Types;
using Ajuna.ServiceLayer.Extensions;
using Ajuna.ServiceLayer.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ajuna.ServiceLayer
{
   public class AjunaSubstrateDataProvider : IStorageDataProvider
   {
      public SubstrateClient Client { get; private set; }

      public AjunaSubstrateDataProvider(SubstrateClient client)
      {
         Client = client;
      }

      public async Task<T> GetStorageAsync<T>(string module, string storageName) where T : IType, new()
      {
         return await Client.GetStorageAsync<T>(module, storageName);
      }

      public async Task<Dictionary<string, T>> GetStorageDictAsync<T>(string module, string storageName) where T : IType, new()
      {
         return await Client.GetStorageDictAsync<T>(module, storageName);
      }
   }
}

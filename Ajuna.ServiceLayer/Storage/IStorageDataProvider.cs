using Ajuna.NetApi.Model.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ajuna.ServiceLayer.Storage
{
   public interface IStorageDataProvider
   {
      Task<Dictionary<string, T>> GetStorageDictAsync<T>(string module, string storageName) where T : IType, new();
      Task<T> GetStorageAsync<T>(string module, string storageName) where T : IType, new();
   }
}

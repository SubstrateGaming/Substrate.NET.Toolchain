using Ajuna.NetApi;
using Ajuna.NetApi.Model.Types;
using Ajuna.ServiceLayer.Extensions;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ajuna.ServiceLayer.Storage
{
   public class TypedMapStorage<T> where T : IType, new()
   {
      internal string Identifier { get; private set; }
      public Dictionary<string, T> Dictionary { get; private set; }
      public IStorageDataProvider DataProvider { get; private set; }
      public IStorageChangeDelegate ChangeDelegate { get; private set; }

      public TypedMapStorage(string identifier, IStorageDataProvider dataProvider)
      {
         Identifier = identifier;
         DataProvider = dataProvider;
      }

      public TypedMapStorage(string identifier, IStorageDataProvider dataProvider, IStorageChangeDelegate changeDelegate)
      {
         Identifier = identifier;
         DataProvider = dataProvider;
         ChangeDelegate = changeDelegate;
      }

      public async Task InitializeAsync(string module, string moduleItem)
      {
         Dictionary = await DataProvider.GetStorageDictAsync<T>(module, moduleItem);
         Log.Information("loaded storage map {storage} with {count} entries", moduleItem, Dictionary.Count);
      }

      public bool ContainsKey(string key)
      {
         return Dictionary.ContainsKey(key);
      }

      public T Get(string key)
      {
         return Dictionary[key];
      }

      public void Update(string key, string data)
      {
         if (string.IsNullOrEmpty(data))
         {
            Dictionary.Remove(key);
            Log.Debug($"[{Identifier}] item {{key}} was deleted.", key);
            ChangeDelegate?.OnDelete(Identifier, key, data);
         }
         else
         {
            var iType = new T();
            iType.Create(data);

            if (Dictionary.ContainsKey(key))
            {
               Dictionary[key] = iType;
               Log.Debug($"[{Identifier}] item {{key}} was updated.", key);
               ChangeDelegate?.OnUpdate(Identifier, key, data);
            }
            else
            {
               Dictionary.Add(key, iType);
               Log.Debug($"[{Identifier}] item {{key}} was created.", key);
               ChangeDelegate?.OnCreate(Identifier, key, data);
            }
         }
      }
   }
}

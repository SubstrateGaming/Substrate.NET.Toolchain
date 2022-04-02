﻿using Ajuna.NetApi;
using Ajuna.NetApi.Model.Types;
using Ajuna.ServiceLayer.Extensions;
using Serilog;
using System.Threading.Tasks;

namespace Ajuna.ServiceLayer.Storage
{
    public class TypedStorage<T> where T : IType, new()
    {
        internal string Identifier { get; private set; }
        public T Store { get; private set; }
        public IStorageChangeDelegate ChangeDelegate { get; private set; }

        public TypedStorage(string identifier)
        {
            Identifier = identifier;
        }

        public TypedStorage(string identifier, IStorageChangeDelegate changeDelegate)
        {
            Identifier = identifier;
            ChangeDelegate = changeDelegate;
        }

        public async Task InitializeAsync(SubstrateClient client, string module, string moduleItem)
        {
            Store = await client.GetStorageAsync<T>(module, moduleItem);
            Log.Information("loaded storage with {name}", moduleItem, Store.ToString());
        }

        public T Get()
        {
            return Store;
        }

        public void Update(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                Log.Debug($"[{Identifier}] item is not set or null.");
            }
            else
            {
                var iType = new T();
                iType.Create(data);

                Store = iType;
                Log.Debug($"[{Identifier}] item was updated.");

                ChangeDelegate?.OnUpdate(Identifier, string.Empty, data);
            }
        }
    }
}

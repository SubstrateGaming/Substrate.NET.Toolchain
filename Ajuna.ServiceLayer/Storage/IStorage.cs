﻿using Ajuna.NetApi;
using System.Threading.Tasks;

namespace Ajuna.ServiceLayer.Storage
{
    public interface IStorage
    {
        Task InitializeAsync(SubstrateClient client);
    }
}

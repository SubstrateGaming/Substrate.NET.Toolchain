using Ajuna.ServiceLayer.Storage;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Ajuna.ServiceLayer
{
    public class GameServiceConfiguration
    {
        public CancellationToken CancellationToken { get; set; }

        public Uri Endpoint { get; set; }

        public List<IStorage> Storages { get; set; }
    }
}

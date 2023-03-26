using Substrate.ServiceLayer.Storage;
using System.Collections.Generic;
using System.Threading;

namespace Substrate.ServiceLayer
{
   public class SubstrateStorageServiceConfiguration
   {
      public CancellationToken CancellationToken { get; set; }

      public IStorageDataProvider DataProvider { get; set; }

      public List<IStorage> Storages { get; set; }

      /// <summary>
      /// If true, the Service Layer will not fetch all initial Storage Values on Startup
      /// </summary>
      public bool IsLazyLoadingEnabled { get; set; } 
   }
}

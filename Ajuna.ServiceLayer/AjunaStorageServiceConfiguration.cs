using Ajuna.ServiceLayer.Storage;
using System.Collections.Generic;
using System.Threading;

namespace Ajuna.ServiceLayer
{
   public class AjunaStorageServiceConfiguration
   {
      public CancellationToken CancellationToken { get; set; }

      public IStorageDataProvider DataProvider { get; set; }

      public List<IStorage> Storages { get; set; }
   }
}

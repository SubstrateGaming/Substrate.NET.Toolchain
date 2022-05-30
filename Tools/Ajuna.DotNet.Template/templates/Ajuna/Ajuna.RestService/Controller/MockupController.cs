using Ajuna.NetApi;
using Ajuna.NetApi.Model.Meta;
using Ajuna.NetApi.Model.Rpc;
using Ajuna.NetApi.Model.Types;
using Ajuna.ServiceLayer.Model;
using Ajuna.ServiceLayer.Storage;
using Ajuna.ServiceLayer.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using static Ajuna.NetApi.Model.Meta.Storage;

namespace Ajuna.RestService.Controller
{
   [Controller]
   [Route("[controller]")]
   [AjunaControllerIgnore]
   public class MockupController : ControllerBase
   {
      private readonly IStorageDataProvider _storageDataProvider;

      public MockupController(IStorageDataProvider storageDataProvider)
      {
         _storageDataProvider = storageDataProvider;
      }

      [HttpPost("data")]
      [Produces("application/json")]
      [ProducesResponseType(typeof(bool), 200)]
      public IActionResult HandleMockupData([FromBody] MockupRequest request)
      {
         if (string.IsNullOrEmpty(request.Key))
         {
            return Ok(false);
         }

         if (request.Value == null)
         {
            return Ok(false);
         }

         try
         {
            string id = string.Empty;

            // We simulate a regular StorageChangeSet here so that processing
            // of the data work the same way as with a regular Substrate client.

            var changeSet = new StorageChangeSet()
            {
               Block = new Ajuna.NetApi.Model.Types.Base.Hash() { },
               Changes = new string[1][]
            };

            string changeData = Utils.Bytes2HexString(request.Value);
            changeSet.Changes[0] = new string[]
            {
               request.Key,
               changeData
            };

            _storageDataProvider.BroadcastLocalStorageChange(id, changeSet);

            return Ok(true);
         }
         catch (Exception)
         {
            // Ignored for now.
            // TODO (svnscha) Will be logged as soon as we expose a configurable logging interface to the RestService template.
         }

         return Ok(false);


      }
   }
}

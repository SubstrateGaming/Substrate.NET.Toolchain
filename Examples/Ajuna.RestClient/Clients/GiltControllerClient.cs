//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Ajuna.RestClient.Clients
{
   using System;
   using System.Threading.Tasks;
   using System.Net.Http;
   using Ajuna.NetApi.Model.Types.Base;
   using Ajuna.NetApi.Model.PalletGilt;
   using Ajuna.RestClient.Interfaces;
   
   public sealed class GiltControllerClient : BaseClient, IGiltControllerClient
   {
      private HttpClient _httpClient;
      public GiltControllerClient(HttpClient httpClient)
      {
         _httpClient = httpClient;
      }
      public async Task<BaseVec<BaseTuple<Ajuna.NetApi.Model.Types.Primitive.U32, Ajuna.NetApi.Model.Types.Primitive.U128>>> GetQueueTotals()
      {
         return await SendRequestAsync<BaseVec<BaseTuple<Ajuna.NetApi.Model.Types.Primitive.U32, Ajuna.NetApi.Model.Types.Primitive.U128>>>(_httpClient, "gilt/queuetotals");
      }
      public async Task<BaseVec<GiltBid>> GetQueues(string key)
      {
         return await SendRequestAsync<BaseVec<GiltBid>>(_httpClient, "gilt/queues", key);
      }
      public async Task<ActiveGiltsTotal> GetActiveTotal()
      {
         return await SendRequestAsync<ActiveGiltsTotal>(_httpClient, "gilt/activetotal");
      }
      public async Task<ActiveGilt> GetActive(string key)
      {
         return await SendRequestAsync<ActiveGilt>(_httpClient, "gilt/active", key);
      }
   }
}

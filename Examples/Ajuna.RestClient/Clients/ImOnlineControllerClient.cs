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
   using Ajuna.NetApi.Model.Types.Primitive;
   using Ajuna.NetApi.Model.FrameSupport;
   using Ajuna.RestClient.Interfaces;
   
   public sealed class ImOnlineControllerClient : BaseClient, IImOnlineControllerClient
   {
      private HttpClient _httpClient;
      public ImOnlineControllerClient(HttpClient httpClient)
      {
         _httpClient = httpClient;
      }
      public async Task<U32> GetHeartbeatAfter()
      {
         return await SendRequestAsync<U32>(_httpClient, "imonline/heartbeatafter");
      }
      public async Task<WeakBoundedVecT4> GetKeys()
      {
         return await SendRequestAsync<WeakBoundedVecT4>(_httpClient, "imonline/keys");
      }
      public async Task<WrapperOpaque> GetReceivedHeartbeats(string key)
      {
         return await SendRequestAsync<WrapperOpaque>(_httpClient, "imonline/receivedheartbeats", key);
      }
      public async Task<U32> GetAuthoredBlocks(string key)
      {
         return await SendRequestAsync<U32>(_httpClient, "imonline/authoredblocks", key);
      }
   }
}

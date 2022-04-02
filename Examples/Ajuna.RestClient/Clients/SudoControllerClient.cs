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
   using Ajuna.NetApi.Model.SpCore;
   using Ajuna.RestClient.Interfaces;
   
   public sealed class SudoControllerClient : BaseClient, ISudoControllerClient
   {
      private HttpClient _httpClient;
      public SudoControllerClient(HttpClient httpClient)
      {
         _httpClient = httpClient;
      }
      public async Task<AccountId32> GetKey()
      {
         return await SendRequestAsync<AccountId32>(_httpClient, "sudo/key");
      }
   }
}

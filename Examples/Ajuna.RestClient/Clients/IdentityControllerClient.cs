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
   using Ajuna.NetApi.Model.PalletIdentity;
   using Ajuna.NetApi.Model.Types.Base;
   using Ajuna.NetApi.Model.FrameSupport;
   using Ajuna.RestClient.Interfaces;
   
   public sealed class IdentityControllerClient : BaseClient, IIdentityControllerClient
   {
      private HttpClient _httpClient;
      public IdentityControllerClient(HttpClient httpClient)
      {
         _httpClient = httpClient;
      }
      public async Task<Registration> GetIdentityOf(string key)
      {
         return await SendRequestAsync<Registration>(_httpClient, "identity/identityof", key);
      }
      public async Task<BaseTuple<Ajuna.NetApi.Model.SpCore.AccountId32, EnumData>> GetSuperOf(string key)
      {
         return await SendRequestAsync<BaseTuple<Ajuna.NetApi.Model.SpCore.AccountId32, EnumData>>(_httpClient, "identity/superof", key);
      }
      public async Task<BaseTuple<Ajuna.NetApi.Model.Types.Primitive.U128, BoundedVecT11>> GetSubsOf(string key)
      {
         return await SendRequestAsync<BaseTuple<Ajuna.NetApi.Model.Types.Primitive.U128, BoundedVecT11>>(_httpClient, "identity/subsof", key);
      }
      public async Task<BoundedVecT12> GetRegistrars()
      {
         return await SendRequestAsync<BoundedVecT12>(_httpClient, "identity/registrars");
      }
   }
}

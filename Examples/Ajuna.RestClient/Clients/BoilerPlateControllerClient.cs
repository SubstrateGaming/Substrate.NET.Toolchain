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
   using Ajuna.RestClient.Interfaces;
   
   public sealed class BoilerPlateControllerClient : BaseClient, IBoilerPlateControllerClient
   {
      private HttpClient _httpClient;
      public BoilerPlateControllerClient(HttpClient httpClient)
      {
         _httpClient = httpClient;
      }
      public async Task<U32> GetSomething()
      {
         return await SendRequestAsync<U32>(_httpClient, "boilerplate/something");
      }
   }
}

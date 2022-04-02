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
   using Ajuna.NetApi.Model.SpArithmetic;
   using Ajuna.NetApi.Model.PalletTransactionPayment;
   using Ajuna.RestClient.Interfaces;
   
   public sealed class TransactionPaymentControllerClient : BaseClient, ITransactionPaymentControllerClient
   {
      private HttpClient _httpClient;
      public TransactionPaymentControllerClient(HttpClient httpClient)
      {
         _httpClient = httpClient;
      }
      public async Task<FixedU128> GetNextFeeMultiplier()
      {
         return await SendRequestAsync<FixedU128>(_httpClient, "transactionpayment/nextfeemultiplier");
      }
      public async Task<EnumReleases> GetStorageVersion()
      {
         return await SendRequestAsync<EnumReleases>(_httpClient, "transactionpayment/storageversion");
      }
   }
}

using Ajuna.NetApi;
using Ajuna.NetApi.Model.SpCore;
using NUnit.Framework;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ajuna.RestClient.Test
{
   public class ClientBasicTest
   {
      private Client _client;

      [SetUp]
      public void Setup()
      {
         var httpClient = new HttpClient()
         {
            BaseAddress = new Uri("http://localhost:5000")
         };
         httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/ajuna");

         _client = new Client(httpClient);
      }

      [Test]
      public async Task GetFounderKeyTestAsync()
      {
         var connectFourClient = _client.ConnectFourControllerClient;
         AccountId32 accountId32 = await connectFourClient.GetFounderKey();

         Assert.AreEqual(
            "0xD43593C715FDD31C61141ABD04A99FD6822C8558854CCDE39A5684E7A56DA27D",
            Utils.Bytes2HexString(accountId32.Value.Bytes));
      }

      //[Test]
      //public async Task GetSystemAccountAsync()
      //{
      //   var systemClient = _client.SystemControllerClient;

      //   AccountId32 accountId32 = new AccountId32();
      //   accountId32.Create(Utils.GetAddressFrom("5GrwvaEF5zXb26Fz9rcQpDWS57CtERHpNehXCPcNoHGKutQY"));


      //   var keyBytes = SystemStorage.AccountParams(accountId32).ToLower();
      //   keyBytes = keyBytes.Substring(2);

      //   var account = await systemClient.GetAccount("0x8eaf04151687736326c9fea17e25fc5287613693c912909cb226aa4794f26a48");

      //   Assert.True(BigInteger.Parse("1100000000000000000000") > account.Data.Free.Value);
      //   Assert.True(BigInteger.Parse("900000000000000000") < account.Data.Free.Value);
      //}
   }
}
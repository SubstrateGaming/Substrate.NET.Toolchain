using Ajuna.NetApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ajuna.RestService.Test
{
   [TestClass]
   public class UnitTest1
   {
      [TestMethod]
      public void TestMethod6()
      {

         string hexString = "8EAF04151687736326C9FEA17E25FC5287613693C912909CB226AA4794F26A48";
         var byteArray = Utils.HexToByteArray(hexString);
         var t = new Ajuna.NetApi.Model.SpCore.AccountId32();
         var p = 0;
         t.Decode(byteArray, ref p);

         Assert.AreEqual("0x8EAF04151687736326C9FEA17E25FC5287613693C912909CB226AA4794F26A48", Utils.Bytes2HexString(t.Value.Bytes));

      }
   }
}

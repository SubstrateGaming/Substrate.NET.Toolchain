using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Ajuna.ServiceLayer.Model;
using System;

namespace Ajuna.RestClient.Mockup
{
   public class MockupBaseClient
   {
      protected async Task<bool> SendMockupRequestAsync(HttpClient client, string storage, byte[] value, byte[] key)
      {
         var request = new MockupRequest()
         {
            Storage = storage,
            Value = value,
            Key = key
         };

         string content = JsonConvert.SerializeObject(request);
         byte[] buffer = Encoding.UTF8.GetBytes(content);
         var byteContent = new ByteArrayContent(buffer);
         byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
         HttpResponseMessage response = await client.PostAsync("mockup/data", byteContent);

         return await ProcessResponseAsync(response);
      }

      protected async Task<bool> SendMockupRequestAsync(HttpClient client, string storage, byte[] value)
      {
         return await SendMockupRequestAsync(client, storage, value, new byte[] { });
      }

      private async Task<bool> ProcessResponseAsync(HttpResponseMessage response)
      {
         if (response == null || !response.IsSuccessStatusCode)
         {
            throw new InvalidOperationException($"Invalid response received while sending request to mockup endpoint!");
         }

         return JsonConvert.DeserializeObject<bool>(await response.Content.ReadAsStringAsync());
      }
   }
}

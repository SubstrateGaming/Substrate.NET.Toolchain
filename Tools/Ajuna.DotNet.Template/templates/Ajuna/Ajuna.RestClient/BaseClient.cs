﻿﻿using Ajuna.NetApi;
using Ajuna.NetApi.Model.Types.Base;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ajuna.RestClient
{
   public class BaseClient
   {
      public class DefaultResponse
      {
         [JsonProperty("result")]
         public string Result { get; set; }
      }

      protected async Task<T> SendRequestAsync<T>(HttpClient client, string endpoint, string key)
      {
         return await SendRequestAsync<T>(client, $"{endpoint}?key={Uri.EscapeDataString(key)}");
      }

      protected async Task<T> SendRequestAsync<T>(HttpClient client, string endpoint)
      {
         var response = await client.GetAsync(endpoint, HttpCompletionOption.ResponseHeadersRead);
         return await ProcessResponseAsync<T>(response, endpoint);
      }
      private async Task<T> ProcessResponseAsync<T>(HttpResponseMessage response, string endpoint)
      {
         if (response == null || !response.IsSuccessStatusCode)
         {
            throw new InvalidOperationException($"Invalid response received while sending request to {endpoint}!");
         }

         var json = JsonConvert.DeserializeObject<DefaultResponse>(await response.Content.ReadAsStringAsync());
         var resultingObject = (T)Activator.CreateInstance(typeof(T));
         var resultObjectBaseType = resultingObject as BaseType;
         resultObjectBaseType.Create(Utils.HexToByteArray(json.Result));

         return resultingObject;
      }
   }
}
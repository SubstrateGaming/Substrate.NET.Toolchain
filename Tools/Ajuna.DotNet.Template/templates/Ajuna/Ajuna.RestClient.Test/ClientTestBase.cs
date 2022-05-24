﻿using Ajuna.NetApi;
using Ajuna.NetApi.Model.Types.Base;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ajuna.RestClient.Test
{
   public class ClientTestBase
   {
      protected HttpClient CreateHttpClient()
      {
         var httpClient = new HttpClient()
         {
            BaseAddress = new Uri("http://localhost:5000")
         };

         httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/ajuna");
         return httpClient;
      }
   }
}

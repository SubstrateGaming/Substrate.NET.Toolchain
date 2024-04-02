using Newtonsoft.Json;

namespace Substrate.DotNet
{
   public class SubstrateConfiguration
   {
      [JsonProperty("projects")]
      public SubstrateConfigurationProjects Projects { get; set; }

      [JsonProperty("metadata")]
      public SubstrateConfigurationMetadata Metadata { get; set; }

      [JsonProperty("rest_client_settings")]
      public SubstrateConfigurationRestClientSettings RestClientSettings { get; set; }
   }

   public class SubstrateConfigurationProjects
   {
      [JsonProperty("net_api")]
      public string NetApi { get; set; }

      [JsonProperty("net_integration")]
      public string Integration { get; set; }

      [JsonProperty("rest_service")]
      public string RestService { get; set; }

      [JsonProperty("rest_client")]
      public string RestClient { get; set; }

      [JsonIgnore]
      public string RestClientMockup
      {
         get
         {
            return $"{RestClient}.Mockup";
         }
      }

      [JsonIgnore]
      public string RestClientTest
      {
         get
         {
            return $"{RestClient}.Test";
         }
      }
   }

   public class SubstrateConfigurationMetadata
   {
      [JsonProperty("websocket")]
      public string Websocket { get; set; }

      [JsonIgnore]
      public string Runtime { get; set; }
   }

   public class SubstrateConfigurationRestClientSettings
   {
      [JsonProperty("service_assembly")]
      public string ServiceAssembly { get; set; }
   }
}

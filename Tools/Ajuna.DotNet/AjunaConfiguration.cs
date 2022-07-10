using Newtonsoft.Json;

namespace Ajuna.DotNet
{
   public class AjunaConfiguration
   {
      [JsonProperty("projects")]
      public AjunaConfigurationProjects Projects { get; set; }

      [JsonProperty("metadata")]
      public AjunaConfigurationMetadata Metadata { get; set; }

      [JsonProperty("rest_client_settings")]
      public AjunaConfigurationRestClientSettings RestClientSettings { get; set; }
   }

   public class AjunaConfigurationProjects
   {
      [JsonProperty("net_api")]
      public string NetApi { get; set; }

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

   public class AjunaConfigurationMetadata
   {
      [JsonProperty("websocket")]
      public string Websocket { get; set; }

      [JsonIgnore]
      public string Runtime { get; set; }
   }

   public class AjunaConfigurationRestClientSettings
   {
      [JsonProperty("service_assembly")]
      public string ServiceAssembly { get; set; }
   }
}

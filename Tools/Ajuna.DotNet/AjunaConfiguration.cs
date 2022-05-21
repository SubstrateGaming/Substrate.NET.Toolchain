using Newtonsoft.Json;

namespace Ajuna.DotNet
{
   public class AjunaConfiguration
   {
      [JsonProperty("projects")]
      public AjunaConfigurationProjects Projects { get; set; }

      [JsonProperty("metadata")]
      public AjunaConfigurationMetadata Metadata { get; set; }
   }

   public class AjunaConfigurationProjects
   {
      [JsonProperty("rest_service")]
      public string RestService { get; set; }

      [JsonProperty("net_api")]
      public string NetApi { get; set; }
   }

   public class AjunaConfigurationMetadata
   {
      [JsonProperty("websocket")]
      public string Websocket { get; set; }

      [JsonProperty("runtime")]
      public string Runtime { get; set; }
   }
}

using Newtonsoft.Json;

namespace Ajuna.ServiceLayer.Model
{
   public class StorageSubscriptionMessage
   {
      [JsonProperty("type")]
      public StorageSubscriptionMessageType Type { get; set; }

      [JsonProperty("payload")]
      public string Payload { get; set; }
   }
}
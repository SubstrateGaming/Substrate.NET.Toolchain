using Newtonsoft.Json;

namespace Ajuna.ServiceLayer.Model
{
   public class MockupRequest
   {
      [JsonProperty("storage")]
      public string Storage { get; set; }

      [JsonProperty("value")]
      public byte[] Value { get; set; }

      [JsonProperty("key")]
      public string Key { get; set; }
   }
}
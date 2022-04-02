using Newtonsoft.Json;

namespace Ajuna.Restclient.Subscription.Model
{
    public class StorageSubscribeMessage
    {
        [JsonProperty("id")]
        public string Identifier { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }
    }
}
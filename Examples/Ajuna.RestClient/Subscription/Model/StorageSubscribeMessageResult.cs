using Newtonsoft.Json;

namespace Ajuna.Restclient.Subscription.Model
{
    public class StorageSubscribeMessageResult
    {
        [JsonProperty("status")]
        public int Status { get; set; }
    }
}
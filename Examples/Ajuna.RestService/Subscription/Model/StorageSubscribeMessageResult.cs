using Newtonsoft.Json;

namespace Ajuna.RestService.Subscription.Model
{
    public class StorageSubscribeMessageResult
    {
        [JsonProperty("status")]
        public int Status { get; set; }
    }
}
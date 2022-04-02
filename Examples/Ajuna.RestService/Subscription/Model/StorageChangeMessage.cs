using Newtonsoft.Json;
using System;

namespace Ajuna.RestService.Subscription.Model
{
    public class StorageChangeMessage
    {
        [JsonProperty("type")]
        public StorageSubscriptionChangeType Type { get; set; }

        [JsonProperty("id")]
        public string Identifier { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
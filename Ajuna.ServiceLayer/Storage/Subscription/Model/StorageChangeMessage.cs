using Newtonsoft.Json;
using System;

namespace Ajuna.ServiceLayer.Storage.Subscription.Model
{
    /// <summary>
    /// This class implements a serializable message to inform web socket clients
    /// about storage changes in real-time.
    /// </summary>
    public class StorageChangeMessage
    {
        /// <summary>
        /// Indicates what type of change this message is about.
        /// </summary>
        [JsonProperty("type")]
        public StorageSubscriptionChangeType Type { get; set; }

        /// <summary>
        /// Indicates the storage identifier.
        /// A typical storage identifier is Assets.Asset or Assets.Account or any other storage with its identifier.
        /// </summary>
        [JsonProperty("id")]
        public string Identifier { get; set; }

        /// <summary>
        /// The key that was changed.
        /// This may be null or empty for storages that do not have a key value based store.
        /// </summary>
        [JsonProperty("key")]
        public string Key { get; set; }

        /// <summary>
        /// The hex-encoded data that was changed.
        /// </summary>
        [JsonProperty("data")]
        public string Data { get; set; }

        /// <summary>
        /// Indicates when the storage change was processed.
        /// </summary>
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
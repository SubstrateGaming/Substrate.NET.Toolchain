using Newtonsoft.Json;

namespace Ajuna.ServiceLayer.Storage.Subscription.Model
{
    /// <summary>
    /// This class implements a serializable message to register for storage changes.
    /// </summary>
    public class StorageSubscribeMessage
    {
        /// <summary>
        /// Indicates the storage identifier.
        /// A typical storage identifier is Assets.Asset or Assets.Account or any other storage with its identifier.
        /// </summary>
        [JsonProperty("id")]
        public string Identifier { get; set; }

        /// <summary>
        /// The key that was changed.
        /// This may be null or empty for storages that do not have a key value based store
        /// or if the client is interested in all changes for that given storage.
        /// </summary>
        [JsonProperty("key")]
        public string Key { get; set; }
    }
}
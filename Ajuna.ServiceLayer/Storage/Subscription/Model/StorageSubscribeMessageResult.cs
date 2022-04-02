using Newtonsoft.Json;

namespace Ajuna.ServiceLayer.Storage.Subscription.Model
{
    /// <summary>
    /// This class implements a serializable status message that indicates the 
    /// actual status of the subscribe operation.
    /// </summary>
    public class StorageSubscribeMessageResult
    {
        /// <summary>
        /// The actual status code that indicates if the subscription was successful or not.
        /// At this point this uses regular HttpStatus codes to indicate its status.
        /// - 200: Success
        /// - 400: BadRequest
        /// </summary>
        [JsonProperty("status")]
        public int Status { get; set; }
    }
}
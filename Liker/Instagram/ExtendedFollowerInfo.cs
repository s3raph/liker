using System.Text.Json.Serialization;

namespace Liker.Instagram
{
    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    public class ExtendedFollowerInfo
    {
        [JsonPropertyName("following")]
        public bool Following { get; set; }

        [JsonPropertyName("incoming_request")]
        public bool IncomingRequest { get; set; }

        [JsonPropertyName("is_bestie")]
        public bool IsBestie { get; set; }

        [JsonPropertyName("is_private")]
        public bool IsPrivate { get; set; }

        [JsonPropertyName("is_restricted")]
        public bool IsRestricted { get; set; }

        [JsonPropertyName("outgoing_request")]
        public bool OutgoingRequest { get; set; }

        [JsonPropertyName("is_feed_favorite")]
        public bool IsFeedFavorite { get; set; }
    }
}

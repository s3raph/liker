using System.Text.Json.Serialization;

namespace Liker.Instagram
{
    internal class FriendShip
    {
        [JsonPropertyName("pk")]
        public string Pk { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("full_name")]
        public string FullName { get; set; }

        [JsonPropertyName("is_private")]
        public bool IsPrivate { get; set; }

        [JsonPropertyName("pk_id")]
        public string PkId { get; set; }

        [JsonPropertyName("profile_pic_url")]
        public string ProfilePicUrl { get; set; }

        [JsonPropertyName("profile_pic_id")]
        public string ProfilePicId { get; set; }

        [JsonPropertyName("is_verified")]
        public bool IsVerified { get; set; }

        [JsonPropertyName("has_anonymous_profile_picture")]
        public bool HasAnonymousProfilePicture { get; set; }

        [JsonPropertyName("has_highlight_reels")]
        public bool HasHighlightReels { get; set; }

        [JsonPropertyName("account_badges")]
        public List<object> AccountBadges { get; set; }

        [JsonPropertyName("latest_reel_media")]
        public int LatestReelMedia { get; set; }
    }
}
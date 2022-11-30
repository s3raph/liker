using System.Text.Json.Serialization;

namespace Liker.Instagram
{
    // Root myDeserializedClass = JsonSerializer.Deserialize<Post>(myJsonResponse);
    public class Candidate
    {
        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }

    public class Caption
    {
        [JsonPropertyName("pk")]
        public string Pk { get; set; }

        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; }

        [JsonPropertyName("created_at")]
        public int CreatedAt { get; set; }

        [JsonPropertyName("created_at_utc")]
        public int CreatedAtUtc { get; set; }

        [JsonPropertyName("content_type")]
        public string ContentType { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("bit_flags")]
        public int BitFlags { get; set; }

        [JsonPropertyName("did_report_as_spam")]
        public bool DidReportAsSpam { get; set; }

        [JsonPropertyName("share_enabled")]
        public bool ShareEnabled { get; set; }

        [JsonPropertyName("user")]
        public User User { get; set; }

        [JsonPropertyName("is_covered")]
        public bool IsCovered { get; set; }

        [JsonPropertyName("is_ranked_comment")]
        public bool IsRankedComment { get; set; }

        [JsonPropertyName("media_id")]
        public string MediaId { get; set; }

        [JsonPropertyName("private_reply_status")]
        public int PrivateReplyStatus { get; set; }
    }

    public class CarouselMedium
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("media_type")]
        public int MediaType { get; set; }

        [JsonPropertyName("image_versions2")]
        public ImageVersions2 ImageVersions2 { get; set; }

        [JsonPropertyName("original_width")]
        public int OriginalWidth { get; set; }

        [JsonPropertyName("original_height")]
        public int OriginalHeight { get; set; }

        [JsonPropertyName("accessibility_caption")]
        public string AccessibilityCaption { get; set; }

        [JsonPropertyName("pk")]
        public string Pk { get; set; }

        [JsonPropertyName("carousel_parent_id")]
        public string CarouselParentId { get; set; }

        [JsonPropertyName("commerciality_status")]
        public string CommercialityStatus { get; set; }

        [JsonPropertyName("sharing_friction_info")]
        public SharingFrictionInfo SharingFrictionInfo { get; set; }
    }

    public class CommentInformTreatment
    {
        [JsonPropertyName("should_have_inform_treatment")]
        public bool ShouldHaveInformTreatment { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("url")]
        public object Url { get; set; }

        [JsonPropertyName("action_type")]
        public object ActionType { get; set; }
    }

    public class FanClubInfo
    {
        [JsonPropertyName("fan_club_id")]
        public object FanClubId { get; set; }

        [JsonPropertyName("fan_club_name")]
        public object FanClubName { get; set; }

        [JsonPropertyName("is_fan_club_referral_eligible")]
        public object IsFanClubReferralEligible { get; set; }

        [JsonPropertyName("fan_consideration_page_revamp_eligiblity")]
        public object FanConsiderationPageRevampEligiblity { get; set; }

        [JsonPropertyName("is_fan_club_gifting_eligible")]
        public object IsFanClubGiftingEligible { get; set; }
    }

    public class ImageVersions2
    {
        [JsonPropertyName("candidates")]
        public List<Candidate> Candidates { get; set; }
    }

    public class Location
    {
        [JsonPropertyName("pk")]
        public string Pk { get; set; }

        [JsonPropertyName("short_name")]
        public string ShortName { get; set; }

        [JsonPropertyName("facebook_places_id")]
        public string FacebookPlacesId { get; set; }

        [JsonPropertyName("external_source")]
        public string ExternalSource { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("has_viewer_saved")]
        public bool HasViewerSaved { get; set; }

        [JsonPropertyName("lng")]
        public double Lng { get; set; }

        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("is_eligible_for_guides")]
        public bool IsEligibleForGuides { get; set; }
    }

    public class MashupInfo
    {
        [JsonPropertyName("mashups_allowed")]
        public bool MashupsAllowed { get; set; }

        [JsonPropertyName("can_toggle_mashups_allowed")]
        public bool CanToggleMashupsAllowed { get; set; }

        [JsonPropertyName("has_been_mashed_up")]
        public bool HasBeenMashedUp { get; set; }

        [JsonPropertyName("formatted_mashups_count")]
        public object FormattedMashupsCount { get; set; }

        [JsonPropertyName("original_media")]
        public object OriginalMedia { get; set; }

        [JsonPropertyName("privacy_filtered_mashups_media_count")]
        public object PrivacyFilteredMashupsMediaCount { get; set; }

        [JsonPropertyName("non_privacy_filtered_mashups_media_count")]
        public object NonPrivacyFilteredMashupsMediaCount { get; set; }

        [JsonPropertyName("mashup_type")]
        public object MashupType { get; set; }

        [JsonPropertyName("is_creator_requesting_mashup")]
        public bool IsCreatorRequestingMashup { get; set; }

        [JsonPropertyName("has_nonmimicable_additional_audio")]
        public bool HasNonmimicableAdditionalAudio { get; set; }
    }

    public class MusicMetadata
    {
        [JsonPropertyName("music_canonical_id")]
        public string MusicCanonicalId { get; set; }

        [JsonPropertyName("audio_type")]
        public object AudioType { get; set; }

        [JsonPropertyName("music_info")]
        public object MusicInfo { get; set; }

        [JsonPropertyName("original_sound_info")]
        public object OriginalSoundInfo { get; set; }

        [JsonPropertyName("pinned_media_ids")]
        public object PinnedMediaIds { get; set; }
    }

    public class Post
    {
        [JsonPropertyName("taken_at")]
        public int TakenAt { get; set; }

        [JsonPropertyName("pk")]
        public string Pk { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        //[JsonPropertyName("device_timestamp")]
        //public long DeviceTimestamp { get; set; }

        [JsonPropertyName("media_type")]
        public int MediaType { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("client_cache_key")]
        public string ClientCacheKey { get; set; }

        [JsonPropertyName("filter_type")]
        public int FilterType { get; set; }

        [JsonPropertyName("is_unified_video")]
        public bool IsUnifiedVideo { get; set; }

        [JsonPropertyName("should_request_ads")]
        public bool ShouldRequestAds { get; set; }

        [JsonPropertyName("original_media_has_visual_reply_media")]
        public bool OriginalMediaHasVisualReplyMedia { get; set; }

        [JsonPropertyName("caption_is_edited")]
        public bool CaptionIsEdited { get; set; }

        [JsonPropertyName("like_and_view_counts_disabled")]
        public bool LikeAndViewCountsDisabled { get; set; }

        [JsonPropertyName("commerciality_status")]
        public string CommercialityStatus { get; set; }

        [JsonPropertyName("is_paid_partnership")]
        public bool IsPaidPartnership { get; set; }

        [JsonPropertyName("is_visual_reply_commenter_notice_enabled")]
        public bool IsVisualReplyCommenterNoticeEnabled { get; set; }

        [JsonPropertyName("clips_tab_pinned_user_ids")]
        public List<object> ClipsTabPinnedUserIds { get; set; }

        [JsonPropertyName("has_delayed_metadata")]
        public bool HasDelayedMetadata { get; set; }

        [JsonPropertyName("location")]
        public Location Location { get; set; }

        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lng")]
        public double Lng { get; set; }

        [JsonPropertyName("comment_likes_enabled")]
        public bool CommentLikesEnabled { get; set; }

        [JsonPropertyName("comment_threading_enabled")]
        public bool CommentThreadingEnabled { get; set; }

        [JsonPropertyName("max_num_visible_preview_comments")]
        public int MaxNumVisiblePreviewComments { get; set; }

        [JsonPropertyName("has_more_comments")]
        public bool HasMoreComments { get; set; }

        [JsonPropertyName("preview_comments")]
        public List<object> PreviewComments { get; set; }

        [JsonPropertyName("comments")]
        public List<object> Comments { get; set; }

        [JsonPropertyName("comment_count")]
        public int CommentCount { get; set; }

        [JsonPropertyName("can_view_more_preview_comments")]
        public bool CanViewMorePreviewComments { get; set; }

        [JsonPropertyName("hide_view_all_comment_entrypoint")]
        public bool HideViewAllCommentEntrypoint { get; set; }

        [JsonPropertyName("inline_composer_display_condition")]
        public string InlineComposerDisplayCondition { get; set; }

        [JsonPropertyName("carousel_media_count")]
        public int CarouselMediaCount { get; set; }

        [JsonPropertyName("carousel_media")]
        public List<CarouselMedium> CarouselMedia { get; set; }

        [JsonPropertyName("can_see_insights_as_brand")]
        public bool CanSeeInsightsAsBrand { get; set; }

        [JsonPropertyName("photo_of_you")]
        public bool PhotoOfYou { get; set; }

        [JsonPropertyName("is_organic_product_tagging_eligible")]
        public bool IsOrganicProductTaggingEligible { get; set; }

        [JsonPropertyName("user")]
        public User User { get; set; }

        [JsonPropertyName("can_viewer_reshare")]
        public bool CanViewerReshare { get; set; }

        [JsonPropertyName("like_count")]
        public int LikeCount { get; set; }

        [JsonPropertyName("has_liked")]
        public bool HasLiked { get; set; }

        [JsonPropertyName("top_likers")]
        public List<string> TopLikers { get; set; }

        [JsonPropertyName("facepile_top_likers")]
        public List<object> FacepileTopLikers { get; set; }

        [JsonPropertyName("mashup_info")]
        public MashupInfo MashupInfo { get; set; }

        [JsonPropertyName("caption")]
        public Caption Caption { get; set; }

        [JsonPropertyName("comment_inform_treatment")]
        public CommentInformTreatment CommentInformTreatment { get; set; }

        [JsonPropertyName("sharing_friction_info")]
        public SharingFrictionInfo SharingFrictionInfo { get; set; }

        [JsonPropertyName("can_viewer_save")]
        public bool CanViewerSave { get; set; }

        [JsonPropertyName("is_in_profile_grid")]
        public bool IsInProfileGrid { get; set; }

        [JsonPropertyName("profile_grid_control_enabled")]
        public bool ProfileGridControlEnabled { get; set; }

        [JsonPropertyName("organic_tracking_token")]
        public string OrganicTrackingToken { get; set; }

        [JsonPropertyName("has_shared_to_fb")]
        public int HasSharedToFb { get; set; }

        [JsonPropertyName("product_type")]
        public string ProductType { get; set; }

        [JsonPropertyName("deleted_reason")]
        public int DeletedReason { get; set; }

        [JsonPropertyName("integrity_review_decision")]
        public string IntegrityReviewDecision { get; set; }

        [JsonPropertyName("commerce_integrity_review_decision")]
        public object CommerceIntegrityReviewDecision { get; set; }

        [JsonPropertyName("music_metadata")]
        public MusicMetadata MusicMetadata { get; set; }

        [JsonPropertyName("is_artist_pick")]
        public bool IsArtistPick { get; set; }
    }

    public class SharingFrictionInfo
    {
        [JsonPropertyName("should_have_sharing_friction")]
        public bool ShouldHaveSharingFriction { get; set; }

        [JsonPropertyName("bloks_app_url")]
        public object BloksAppUrl { get; set; }

        [JsonPropertyName("sharing_friction_payload")]
        public object SharingFrictionPayload { get; set; }
    }

    public class User
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

        [JsonPropertyName("is_unpublished")]
        public bool IsUnpublished { get; set; }

        [JsonPropertyName("is_favorite")]
        public bool IsFavorite { get; set; }

        [JsonPropertyName("has_highlight_reels")]
        public bool HasHighlightReels { get; set; }

        [JsonPropertyName("transparency_product_enabled")]
        public bool TransparencyProductEnabled { get; set; }

        [JsonPropertyName("account_badges")]
        public List<object> AccountBadges { get; set; }

        [JsonPropertyName("fan_club_info")]
        public FanClubInfo FanClubInfo { get; set; }

        [JsonPropertyName("latest_reel_media")]
        public int LatestReelMedia { get; set; }
    }
}
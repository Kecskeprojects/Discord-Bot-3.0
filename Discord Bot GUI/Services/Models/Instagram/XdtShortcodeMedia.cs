using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Instagram;
public class XdtShortcodeMedia
{
    [JsonProperty("__typename")]
    [JsonPropertyName("__typename")]
    public string Typename { get; set; }

    [JsonProperty("__isXDTGraphMediaInterface")]
    [JsonPropertyName("__isXDTGraphMediaInterface")]
    public string IsXDTGraphMediaInterface { get; set; }

    [JsonProperty("id")]
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonProperty("shortcode")]
    [JsonPropertyName("shortcode")]
    public string Shortcode { get; set; }

    [JsonProperty("thumbnail_src")]
    [JsonPropertyName("thumbnail_src")]
    public string ThumbnailSrc { get; set; }

    [JsonProperty("dimensions")]
    [JsonPropertyName("dimensions")]
    public Dimensions Dimensions { get; set; }

    [JsonProperty("gating_info")]
    [JsonPropertyName("gating_info")]
    public object GatingInfo { get; set; }

    [JsonProperty("fact_check_overall_rating")]
    [JsonPropertyName("fact_check_overall_rating")]
    public object FactCheckOverallRating { get; set; }

    [JsonProperty("fact_check_information")]
    [JsonPropertyName("fact_check_information")]
    public object FactCheckInformation { get; set; }

    [JsonProperty("sensitivity_friction_info")]
    [JsonPropertyName("sensitivity_friction_info")]
    public object SensitivityFrictionInfo { get; set; }

    [JsonProperty("sharing_friction_info")]
    [JsonPropertyName("sharing_friction_info")]
    public SharingFrictionInfo SharingFrictionInfo { get; set; }

    [JsonProperty("media_overlay_info")]
    [JsonPropertyName("media_overlay_info")]
    public object MediaOverlayInfo { get; set; }

    [JsonProperty("media_preview")]
    [JsonPropertyName("media_preview")]
    public string MediaPreview { get; set; }

    [JsonProperty("display_url")]
    [JsonPropertyName("display_url")]
    public string DisplayUrl { get; set; }

    [JsonProperty("display_resources")]
    [JsonPropertyName("display_resources")]
    public List<DisplayResource> DisplayResources { get; set; }

    [JsonProperty("dash_info")]
    [JsonPropertyName("dash_info")]
    public DashInfo DashInfo { get; set; }

    [JsonProperty("has_audio")]
    [JsonPropertyName("has_audio")]
    public bool HasAudio { get; set; }

    [JsonProperty("video_url")]
    [JsonPropertyName("video_url")]
    public string VideoUrl { get; set; }

    [JsonProperty("video_view_count")]
    [JsonPropertyName("video_view_count")]
    public int VideoViewCount { get; set; }

    [JsonProperty("video_play_count")]
    [JsonPropertyName("video_play_count")]
    public int VideoPlayCount { get; set; }

    [JsonProperty("encoding_status")]
    [JsonPropertyName("encoding_status")]
    public object EncodingStatus { get; set; }

    [JsonProperty("is_published")]
    [JsonPropertyName("is_published")]
    public bool IsPublished { get; set; }

    [JsonProperty("product_type")]
    [JsonPropertyName("product_type")]
    public string ProductType { get; set; }

    [JsonProperty("title")]
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonProperty("video_duration")]
    [JsonPropertyName("video_duration")]
    public double VideoDuration { get; set; }

    [JsonProperty("clips_music_attribution_info")]
    [JsonPropertyName("clips_music_attribution_info")]
    public ClipsMusicAttributionInfo ClipsMusicAttributionInfo { get; set; }

    [JsonProperty("is_video")]
    [JsonPropertyName("is_video")]
    public bool IsVideo { get; set; }

    [JsonProperty("tracking_token")]
    [JsonPropertyName("tracking_token")]
    public string TrackingToken { get; set; }

    [JsonProperty("upcoming_event")]
    [JsonPropertyName("upcoming_event")]
    public object UpcomingEvent { get; set; }

    [JsonProperty("edge_media_to_tagged_user")]
    [JsonPropertyName("edge_media_to_tagged_user")]
    public EdgeMediaToTaggedUser EdgeMediaToTaggedUser { get; set; }

    [JsonProperty("owner")]
    [JsonPropertyName("owner")]
    public Owner Owner { get; set; }

    [JsonProperty("accessibility_caption")]
    [JsonPropertyName("accessibility_caption")]
    public string AccessibilityCaption { get; set; }

    [JsonProperty("edge_sidecar_to_children")]
    [JsonPropertyName("edge_sidecar_to_children")]
    public EdgeSidecarToChildren EdgeSidecarToChildren { get; set; }

    [JsonProperty("edge_media_to_caption")]
    [JsonPropertyName("edge_media_to_caption")]
    public EdgeMediaToCaption EdgeMediaToCaption { get; set; }

    [JsonProperty("can_see_insights_as_brand")]
    [JsonPropertyName("can_see_insights_as_brand")]
    public bool CanSeeInsightsAsBrand { get; set; }

    [JsonProperty("caption_is_edited")]
    [JsonPropertyName("caption_is_edited")]
    public bool CaptionIsEdited { get; set; }

    [JsonProperty("has_ranked_comments")]
    [JsonPropertyName("has_ranked_comments")]
    public bool HasRankedComments { get; set; }

    [JsonProperty("like_and_view_counts_disabled")]
    [JsonPropertyName("like_and_view_counts_disabled")]
    public bool LikeAndViewCountsDisabled { get; set; }

    [JsonProperty("edge_media_to_parent_comment")]
    [JsonPropertyName("edge_media_to_parent_comment")]
    public EdgeMediaToParentComment EdgeMediaToParentComment { get; set; }

    [JsonProperty("edge_media_to_hoisted_comment")]
    [JsonPropertyName("edge_media_to_hoisted_comment")]
    public EdgeMediaToHoistedComment EdgeMediaToHoistedComment { get; set; }

    [JsonProperty("edge_media_preview_comment")]
    [JsonPropertyName("edge_media_preview_comment")]
    public EdgeMediaPreviewComment EdgeMediaPreviewComment { get; set; }

    [JsonProperty("comments_disabled")]
    [JsonPropertyName("comments_disabled")]
    public bool CommentsDisabled { get; set; }

    [JsonProperty("commenting_disabled_for_viewer")]
    [JsonPropertyName("commenting_disabled_for_viewer")]
    public bool CommentingDisabledForViewer { get; set; }

    [JsonProperty("taken_at_timestamp")]
    [JsonPropertyName("taken_at_timestamp")]
    public int TakenAtTimestamp { get; set; }

    [JsonProperty("edge_media_preview_like")]
    [JsonPropertyName("edge_media_preview_like")]
    public EdgeMediaPreviewLike EdgeMediaPreviewLike { get; set; }

    [JsonProperty("edge_media_to_sponsor_user")]
    [JsonPropertyName("edge_media_to_sponsor_user")]
    public EdgeMediaToSponsorUser EdgeMediaToSponsorUser { get; set; }

    [JsonProperty("is_affiliate")]
    [JsonPropertyName("is_affiliate")]
    public bool IsAffiliate { get; set; }

    [JsonProperty("is_paid_partnership")]
    [JsonPropertyName("is_paid_partnership")]
    public bool IsPaidPartnership { get; set; }

    [JsonProperty("location")]
    [JsonPropertyName("location")]
    public object Location { get; set; }

    [JsonProperty("nft_asset_info")]
    [JsonPropertyName("nft_asset_info")]
    public object NftAssetInfo { get; set; }

    [JsonProperty("viewer_has_liked")]
    [JsonPropertyName("viewer_has_liked")]
    public bool ViewerHasLiked { get; set; }

    [JsonProperty("viewer_has_saved")]
    [JsonPropertyName("viewer_has_saved")]
    public bool ViewerHasSaved { get; set; }

    [JsonProperty("viewer_has_saved_to_collection")]
    [JsonPropertyName("viewer_has_saved_to_collection")]
    public bool ViewerHasSavedToCollection { get; set; }

    [JsonProperty("viewer_in_photo_of_you")]
    [JsonPropertyName("viewer_in_photo_of_you")]
    public bool ViewerInPhotoOfYou { get; set; }

    [JsonProperty("viewer_can_reshare")]
    [JsonPropertyName("viewer_can_reshare")]
    public bool ViewerCanReshare { get; set; }

    [JsonProperty("is_ad")]
    [JsonPropertyName("is_ad")]
    public bool IsAd { get; set; }

    [JsonProperty("edge_web_media_to_related_media")]
    [JsonPropertyName("edge_web_media_to_related_media")]
    public EdgeWebMediaToRelatedMedia EdgeWebMediaToRelatedMedia { get; set; }

    [JsonProperty("coauthor_producers")]
    [JsonPropertyName("coauthor_producers")]
    public List<object> CoauthorProducers { get; set; }

    [JsonProperty("pinned_for_users")]
    [JsonPropertyName("pinned_for_users")]
    public List<object> PinnedForUsers { get; set; }
}

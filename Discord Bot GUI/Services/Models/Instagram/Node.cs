using Discord_Bot.Services.Models.Instagram.Edge;
using Discord_Bot.Services.Models.Instagram.OtherSubClasses;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Instagram;

public class Node
{
    [JsonPropertyName("__typename")]
    public string Typename { get; set; }

    [JsonPropertyName("caption_is_edited")]
    public bool CaptionIsEdited { get; set; }

    [JsonPropertyName("commenting_disabled_for_viewer")]
    public bool CommentingDisabledForViewer { get; set; }

    [JsonPropertyName("comments_disabled")]
    public bool CommentsDisabled { get; set; }

    [JsonPropertyName("dimensions")]
    public Dimensions Dimensions { get; set; }

    [JsonPropertyName("display_resources")]
    public List<DisplayResource> DisplayResources { get; set; }

    [JsonPropertyName("display_url")]
    public string DisplayUrl { get; set; }

    [JsonPropertyName("edge_media_preview_like")]
    public EdgeMediaPreviewLike EdgeMediaPreviewLike { get; set; }

    [JsonPropertyName("edge_media_to_caption")]
    public EdgeMediaToCaption EdgeMediaToCaption { get; set; }

    [JsonPropertyName("edge_media_to_comment")]
    public EdgeMediaToComment EdgeMediaToComment { get; set; }

    [JsonPropertyName("edge_media_to_sponsor_user")]
    public EdgeMediaToSponsorUser EdgeMediaToSponsorUser { get; set; }

    [JsonPropertyName("edge_media_to_tagged_user")]
    public EdgeMediaToTaggedUser EdgeMediaToTaggedUser { get; set; }

    [JsonPropertyName("edge_sidecar_to_children")]
    public EdgeSidecarToChildren EdgeSidecarToChildren { get; set; }

    [JsonPropertyName("edge_web_media_to_related_media")]
    public EdgeWebMediaToRelatedMedia EdgeWebMediaToRelatedMedia { get; set; }

    [JsonPropertyName("fact_check_information")]
    public object FactCheckInformation { get; set; }

    [JsonPropertyName("fact_check_overall_rating")]
    public object FactCheckOverallRating { get; set; }

    [JsonPropertyName("gating_info")]
    public object GatingInfo { get; set; }

    [JsonPropertyName("has_ranked_comments")]
    public bool HasRankedComments { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("is_ad")]
    public bool IsAd { get; set; }

    [JsonPropertyName("is_video")]
    public bool IsVideo { get; set; }

    [JsonPropertyName("location")]
    public object Location { get; set; }

    [JsonPropertyName("media_preview")]
    public object MediaPreview { get; set; }

    [JsonPropertyName("owner")]
    public Owner Owner { get; set; }

    [JsonPropertyName("shortcode")]
    public string Shortcode { get; set; }

    [JsonPropertyName("taken_at_timestamp")]
    public int TakenAtTimestamp { get; set; }

    [JsonPropertyName("tracking_token")]
    public string TrackingToken { get; set; }

    [JsonPropertyName("viewer_can_reshare")]
    public bool ViewerCanReshare { get; set; }

    [JsonPropertyName("viewer_has_liked")]
    public bool ViewerHasLiked { get; set; }

    [JsonPropertyName("viewer_has_saved")]
    public bool ViewerHasSaved { get; set; }

    [JsonPropertyName("viewer_has_saved_to_collection")]
    public bool ViewerHasSavedToCollection { get; set; }

    [JsonPropertyName("viewer_in_photo_of_you")]
    public bool ViewerInPhotoOfYou { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("user")]
    public User User { get; set; }

    [JsonPropertyName("x")]
    public double X { get; set; }

    [JsonPropertyName("y")]
    public double Y { get; set; }

    [JsonPropertyName("accessibility_caption")]
    public object AccessibilityCaption { get; set; }

    [JsonPropertyName("dash_info")]
    public DashInfo DashInfo { get; set; }

    [JsonPropertyName("video_url")]
    public string VideoUrl { get; set; }

    [JsonPropertyName("video_view_count")]
    public int? VideoViewCount { get; set; }
}

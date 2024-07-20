using Discord_Bot.Services.Models.Instagram.Edge;
using Discord_Bot.Services.Models.Instagram.OtherSubClasses;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Instagram;
public class Node
{
    [JsonProperty("__typename")]
    public string Typename { get; set; }

    [JsonProperty("caption_is_edited")]
    public bool CaptionIsEdited { get; set; }

    [JsonProperty("commenting_disabled_for_viewer")]
    public bool CommentingDisabledForViewer { get; set; }

    [JsonProperty("comments_disabled")]
    public bool CommentsDisabled { get; set; }

    [JsonProperty("dimensions")]
    public Dimensions Dimensions { get; set; }

    [JsonProperty("display_resources")]
    public List<DisplayResource> DisplayResources { get; set; }

    [JsonProperty("display_url")]
    public string DisplayUrl { get; set; }

    [JsonProperty("edge_media_preview_like")]
    public EdgeMediaPreviewLike EdgeMediaPreviewLike { get; set; }

    [JsonProperty("edge_media_to_caption")]
    public EdgeMediaToCaption EdgeMediaToCaption { get; set; }

    [JsonProperty("edge_media_to_comment")]
    public EdgeMediaToComment EdgeMediaToComment { get; set; }

    [JsonProperty("edge_media_to_sponsor_user")]
    public EdgeMediaToSponsorUser EdgeMediaToSponsorUser { get; set; }

    [JsonProperty("edge_media_to_tagged_user")]
    public EdgeMediaToTaggedUser EdgeMediaToTaggedUser { get; set; }

    [JsonProperty("edge_sidecar_to_children")]
    public EdgeSidecarToChildren EdgeSidecarToChildren { get; set; }

    [JsonProperty("edge_web_media_to_related_media")]
    public EdgeWebMediaToRelatedMedia EdgeWebMediaToRelatedMedia { get; set; }

    [JsonProperty("fact_check_information")]
    public object FactCheckInformation { get; set; }

    [JsonProperty("fact_check_overall_rating")]
    public object FactCheckOverallRating { get; set; }

    [JsonProperty("gating_info")]
    public object GatingInfo { get; set; }

    [JsonProperty("has_ranked_comments")]
    public bool HasRankedComments { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("is_ad")]
    public bool IsAd { get; set; }

    [JsonProperty("is_video")]
    public bool IsVideo { get; set; }

    [JsonProperty("location")]
    public object Location { get; set; }

    [JsonProperty("media_preview")]
    public object MediaPreview { get; set; }

    [JsonProperty("owner")]
    public Owner Owner { get; set; }

    [JsonProperty("shortcode")]
    public string Shortcode { get; set; }

    [JsonProperty("taken_at_timestamp")]
    public int TakenAtTimestamp { get; set; }

    [JsonProperty("tracking_token")]
    public string TrackingToken { get; set; }

    [JsonProperty("viewer_can_reshare")]
    public bool ViewerCanReshare { get; set; }

    [JsonProperty("viewer_has_liked")]
    public bool ViewerHasLiked { get; set; }

    [JsonProperty("viewer_has_saved")]
    public bool ViewerHasSaved { get; set; }

    [JsonProperty("viewer_has_saved_to_collection")]
    public bool ViewerHasSavedToCollection { get; set; }

    [JsonProperty("viewer_in_photo_of_you")]
    public bool ViewerInPhotoOfYou { get; set; }

    [JsonProperty("text")]
    public string Text { get; set; }

    [JsonProperty("user")]
    public User User { get; set; }

    [JsonProperty("x")]
    public double X { get; set; }

    [JsonProperty("y")]
    public double Y { get; set; }

    [JsonProperty("accessibility_caption")]
    public object AccessibilityCaption { get; set; }

    [JsonProperty("dash_info")]
    public DashInfo DashInfo { get; set; }

    [JsonProperty("video_url")]
    public string VideoUrl { get; set; }

    [JsonProperty("video_view_count")]
    public int? VideoViewCount { get; set; }
}

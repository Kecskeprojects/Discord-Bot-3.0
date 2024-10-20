using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Instagram;
public class Node
{
    [JsonProperty("__typename")]
    [JsonPropertyName("__typename")]
    public string Typename { get; set; }

    [JsonProperty("id")]
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonProperty("shortcode")]
    [JsonPropertyName("shortcode")]
    public string Shortcode { get; set; }

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

    [JsonProperty("accessibility_caption")]
    [JsonPropertyName("accessibility_caption")]
    public string AccessibilityCaption { get; set; }

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

    [JsonProperty("created_at")]
    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; }

    [JsonProperty("text")]
    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonProperty("id")]
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonProperty("did_report_as_spam")]
    [JsonPropertyName("did_report_as_spam")]
    public bool DidReportAsSpam { get; set; }

    [JsonProperty("owner")]
    [JsonPropertyName("owner")]
    public Owner Owner { get; set; }

    [JsonProperty("viewer_has_liked")]
    [JsonPropertyName("viewer_has_liked")]
    public bool ViewerHasLiked { get; set; }

    [JsonProperty("edge_liked_by")]
    [JsonPropertyName("edge_liked_by")]
    public EdgeLikedBy EdgeLikedBy { get; set; }

    [JsonProperty("is_restricted_pending")]
    [JsonPropertyName("is_restricted_pending")]
    public bool IsRestrictedPending { get; set; }

    [JsonProperty("edge_threaded_comments")]
    [JsonPropertyName("edge_threaded_comments")]
    public EdgeThreadedComments EdgeThreadedComments { get; set; }
}

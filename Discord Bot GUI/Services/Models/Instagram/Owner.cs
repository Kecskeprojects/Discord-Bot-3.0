using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Instagram;
public class Owner
{
    [JsonProperty("id")]
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonProperty("username")]
    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonProperty("is_verified")]
    [JsonPropertyName("is_verified")]
    public bool IsVerified { get; set; }

    [JsonProperty("profile_pic_url")]
    [JsonPropertyName("profile_pic_url")]
    public string ProfilePicUrl { get; set; }

    [JsonProperty("blocked_by_viewer")]
    [JsonPropertyName("blocked_by_viewer")]
    public bool BlockedByViewer { get; set; }

    [JsonProperty("restricted_by_viewer")]
    [JsonPropertyName("restricted_by_viewer")]
    public object RestrictedByViewer { get; set; }

    [JsonProperty("followed_by_viewer")]
    [JsonPropertyName("followed_by_viewer")]
    public bool FollowedByViewer { get; set; }

    [JsonProperty("full_name")]
    [JsonPropertyName("full_name")]
    public string FullName { get; set; }

    [JsonProperty("has_blocked_viewer")]
    [JsonPropertyName("has_blocked_viewer")]
    public bool HasBlockedViewer { get; set; }

    [JsonProperty("is_embeds_disabled")]
    [JsonPropertyName("is_embeds_disabled")]
    public bool IsEmbedsDisabled { get; set; }

    [JsonProperty("is_private")]
    [JsonPropertyName("is_private")]
    public bool IsPrivate { get; set; }

    [JsonProperty("is_unpublished")]
    [JsonPropertyName("is_unpublished")]
    public bool IsUnpublished { get; set; }

    [JsonProperty("requested_by_viewer")]
    [JsonPropertyName("requested_by_viewer")]
    public bool RequestedByViewer { get; set; }

    [JsonProperty("pass_tiering_recommendation")]
    [JsonPropertyName("pass_tiering_recommendation")]
    public bool PassTieringRecommendation { get; set; }

    [JsonProperty("edge_owner_to_timeline_media")]
    [JsonPropertyName("edge_owner_to_timeline_media")]
    public EdgeOwnerToTimelineMedia EdgeOwnerToTimelineMedia { get; set; }

    [JsonProperty("edge_followed_by")]
    [JsonPropertyName("edge_followed_by")]
    public EdgeFollowedBy EdgeFollowedBy { get; set; }
}

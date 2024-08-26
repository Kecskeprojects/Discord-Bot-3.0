using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Instagram.OtherSubClasses;

public class Owner
{
    [JsonProperty("blocked_by_viewer")]
    [JsonPropertyName("blocked_by_viewer")]
    public bool BlockedByViewer { get; set; }

    [JsonProperty("followed_by_viewer")]
    [JsonPropertyName("followed_by_viewer")]
    public bool FollowedByViewer { get; set; }

    [JsonProperty("full_name")]
    [JsonPropertyName("full_name")]
    public string FullName { get; set; }

    [JsonProperty("has_blocked_viewer")]
    [JsonPropertyName("has_blocked_viewer")]
    public bool HasBlockedViewer { get; set; }

    [JsonProperty("id")]
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonProperty("is_private")]
    [JsonPropertyName("is_private")]
    public bool IsPrivate { get; set; }

    [JsonProperty("is_unpublished")]
    [JsonPropertyName("is_unpublished")]
    public bool IsUnpublished { get; set; }

    [JsonProperty("is_verified")]
    [JsonPropertyName("is_verified")]
    public bool IsVerified { get; set; }

    [JsonProperty("profile_pic_url")]
    [JsonPropertyName("profile_pic_url")]
    public string ProfilePicUrl { get; set; }

    [JsonProperty("requested_by_viewer")]
    [JsonPropertyName("requested_by_viewer")]
    public bool RequestedByViewer { get; set; }

    [JsonProperty("username")]
    [JsonPropertyName("username")]
    public string Username { get; set; }
}

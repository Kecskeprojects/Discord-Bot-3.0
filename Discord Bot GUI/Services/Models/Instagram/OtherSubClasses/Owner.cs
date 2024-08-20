using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Instagram.OtherSubClasses;

public class Owner
{
    [JsonPropertyName("blocked_by_viewer")]
    public bool BlockedByViewer { get; set; }

    [JsonPropertyName("followed_by_viewer")]
    public bool FollowedByViewer { get; set; }

    [JsonPropertyName("full_name")]
    public string FullName { get; set; }

    [JsonPropertyName("has_blocked_viewer")]
    public bool HasBlockedViewer { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("is_private")]
    public bool IsPrivate { get; set; }

    [JsonPropertyName("is_unpublished")]
    public bool IsUnpublished { get; set; }

    [JsonPropertyName("is_verified")]
    public bool IsVerified { get; set; }

    [JsonPropertyName("profile_pic_url")]
    public string ProfilePicUrl { get; set; }

    [JsonPropertyName("requested_by_viewer")]
    public bool RequestedByViewer { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }
}

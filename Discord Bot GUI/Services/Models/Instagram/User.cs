using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Instagram;

public class User
{
    [JsonProperty("full_name")]
    [JsonPropertyName("full_name")]
    public string FullName { get; set; }

    [JsonProperty("followed_by_viewer")]
    [JsonPropertyName("followed_by_viewer")]
    public bool FollowedByViewer { get; set; }

    [JsonProperty("id")]
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonProperty("is_verified")]
    [JsonPropertyName("is_verified")]
    public bool IsVerified { get; set; }

    [JsonProperty("profile_pic_url")]
    [JsonPropertyName("profile_pic_url")]
    public string ProfilePicUrl { get; set; }

    [JsonProperty("username")]
    [JsonPropertyName("username")]
    public string Username { get; set; }
}

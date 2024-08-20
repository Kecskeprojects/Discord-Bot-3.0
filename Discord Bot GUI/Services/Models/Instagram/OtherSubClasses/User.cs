using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Instagram.OtherSubClasses;

public class User
{
    [JsonPropertyName("full_name")]
    public string FullName { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("is_verified")]
    public bool IsVerified { get; set; }

    [JsonPropertyName("profile_pic_url")]
    public string ProfilePicUrl { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }
}

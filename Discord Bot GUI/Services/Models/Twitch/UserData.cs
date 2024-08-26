using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitch;

public class UserData
{
    [JsonProperty("broadcaster_type")]
    [JsonPropertyName("broadcaster_type")]
    public string BroadcasterType { get; set; }

    [JsonProperty("created_at")]
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("description")]
    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonProperty("display_name")]
    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; }

    [JsonProperty("id")]
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonProperty("login")]
    [JsonPropertyName("login")]
    public string Login { get; set; }

    [JsonProperty("offline_image_url")]
    [JsonPropertyName("offline_image_url")]
    public string OfflineImageUrl { get; set; }

    [JsonProperty("profile_image_url")]
    [JsonPropertyName("profile_image_url")]
    public string ProfileImageUrl { get; set; }

    [JsonProperty("type")]
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonProperty("view_count")]
    [JsonPropertyName("view_count")]
    public int ViewCount { get; set; }
}

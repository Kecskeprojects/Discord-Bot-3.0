﻿using Newtonsoft.Json;
using System;

namespace Discord_Bot.Services.Models.Twitch;
public class UserData
{
    [JsonProperty("broadcaster_type")]
    public string BroadcasterType { get; set; }

    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("display_name")]
    public string DisplayName { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("login")]
    public string Login { get; set; }

    [JsonProperty("offline_image_url")]
    public string OfflineImageUrl { get; set; }

    [JsonProperty("profile_image_url")]
    public string ProfileImageUrl { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("view_count")]
    public int ViewCount { get; set; }
}

﻿using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.InstaLoader.OtherSubClasses;

public class PageInfo
{
    [JsonProperty("end_cursor")]
    [JsonPropertyName("end_cursor")]
    public string EndCursor { get; set; }

    [JsonProperty("has_next_page")]
    [JsonPropertyName("has_next_page")]
    public bool HasNextPage { get; set; }
}

﻿using Discord_Bot.Services.Models.InstaLoader.OtherSubClasses;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.InstaLoader.Edge;

public class EdgeMediaToComment
{
    [JsonProperty("count")]
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonProperty("edges")]
    [JsonPropertyName("edges")]
    public List<object> Edges { get; set; }

    [JsonProperty("page_info")]
    [JsonPropertyName("page_info")]
    public PageInfo PageInfo { get; set; }
}
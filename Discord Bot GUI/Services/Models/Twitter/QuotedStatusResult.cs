﻿using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class QuotedStatusResult
{
    [JsonProperty("result")]
    [JsonPropertyName("result")]
    public Result Result { get; set; }
}

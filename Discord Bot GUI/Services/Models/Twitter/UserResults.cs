﻿using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class UserResults
{
    [JsonProperty("result")]
    [JsonPropertyName("result")]
    public Result Result { get; set; }
}

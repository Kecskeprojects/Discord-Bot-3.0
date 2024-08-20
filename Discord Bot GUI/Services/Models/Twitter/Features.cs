﻿using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class Features
{
    [JsonPropertyName("large")]
    public Large Large { get; set; }

    [JsonPropertyName("medium")]
    public Medium Medium { get; set; }

    [JsonPropertyName("small")]
    public Small Small { get; set; }

    [JsonPropertyName("orig")]
    public Orig Orig { get; set; }
}

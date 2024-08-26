using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class VideoInfo
{
    [JsonProperty("aspect_ratio")]
    [JsonPropertyName("aspect_ratio")]
    public List<int> AspectRatio { get; set; }

    [JsonProperty("duration_millis")]
    [JsonPropertyName("duration_millis")]
    public int DurationMillis { get; set; }

    [JsonProperty("variants")]
    [JsonPropertyName("variants")]
    public List<Variant> Variants { get; set; }
}

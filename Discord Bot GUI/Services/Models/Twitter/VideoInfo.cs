using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Twitter;

public class VideoInfo
{
    [JsonPropertyName("aspect_ratio")]
    public List<int> AspectRatio { get; set; }

    [JsonPropertyName("duration_millis")]
    public int DurationMillis { get; set; }

    [JsonPropertyName("variants")]
    public List<Variant> Variants { get; set; }
}

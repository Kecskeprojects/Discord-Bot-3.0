using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class Variant
{
    [JsonPropertyName("bitrate")]
    public int Bitrate { get; set; }

    [JsonPropertyName("content_type")]
    public string ContentType { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
}

using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class Variant
{
    [JsonProperty("bitrate")]
    [JsonPropertyName("bitrate")]
    public int Bitrate { get; set; }

    [JsonProperty("content_type")]
    [JsonPropertyName("content_type")]
    public string ContentType { get; set; }

    [JsonProperty("url")]
    [JsonPropertyName("url")]
    public string Url { get; set; }
}

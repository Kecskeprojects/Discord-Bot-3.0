using Newtonsoft.Json;

namespace Discord_Bot.Services.Models.Twitter;
public class Variant
{
    [JsonProperty("bitrate")]
    public int Bitrate { get; set; }

    [JsonProperty("content_type")]
    public string ContentType { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }
}

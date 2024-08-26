using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.ArtistInfo;

public class Stats
{
    [JsonProperty("listeners")]
    [JsonPropertyName("listeners")]
    public string Listeners { get; set; }

    [JsonProperty("playcount")]
    [JsonPropertyName("playcount")]
    public string Playcount { get; set; }

    [JsonProperty("userplaycount")]
    [JsonPropertyName("userplaycount")]
    public string Userplaycount { get; set; }
}

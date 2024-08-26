using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.TopTrack;

public class Artist
{
    [JsonProperty("url")]
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonProperty("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonProperty("mbid")]
    [JsonPropertyName("mbid")]
    public string Mbid { get; set; }
}

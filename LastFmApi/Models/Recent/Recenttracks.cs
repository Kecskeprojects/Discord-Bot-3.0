using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.Recent;

public class Recenttracks
{
    [JsonProperty("track")]
    [JsonPropertyName("track")]
    public List<Track> Track { get; set; }

    [JsonProperty("@attr")]
    [JsonPropertyName("@attr")]
    public Attr Attr { get; set; }
}

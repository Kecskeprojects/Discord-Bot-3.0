using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.TopTrack;

public class Toptracks
{
    [JsonProperty("track")]
    [JsonPropertyName("track")]
    public List<Track> Track { get; set; }

    [JsonProperty("@attr")]
    [JsonPropertyName("@attr")]
    public Attr Attr { get; set; }
}

using System.Text.Json.Serialization;

namespace LastFmApi.Models.TopTrack;

public class Toptracks
{
    [JsonPropertyName("track")]
    public List<Track> Track { get; set; }

    [JsonPropertyName("@attr")]
    public Attr Attr { get; set; }
}

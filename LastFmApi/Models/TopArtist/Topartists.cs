using System.Text.Json.Serialization;

namespace LastFmApi.Models.TopArtist;

public class Topartists
{
    [JsonPropertyName("artist")]
    public List<Artist> Artist { get; set; }

    [JsonPropertyName("@attr")]
    public Attr Attr { get; set; }
}

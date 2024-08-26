using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.TopArtist;

public class Topartists
{
    [JsonProperty("artist")]
    [JsonPropertyName("artist")]
    public List<Artist> Artist { get; set; }

    [JsonProperty("@attr")]
    [JsonPropertyName("@attr")]
    public Attr Attr { get; set; }
}

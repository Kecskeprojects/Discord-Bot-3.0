using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.TopAlbum;

public class Topalbums
{
    [JsonProperty("album")]
    [JsonPropertyName("album")]
    public List<Album> Album { get; set; }

    [JsonProperty("@attr")]
    [JsonPropertyName("@attr")]
    public Attr Attr { get; set; }
}

using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.ArtistInfo;

public class Image
{
    [JsonProperty("#text")]
    [JsonPropertyName("#text")]
    public string Text { get; set; }

    [JsonProperty("size")]
    [JsonPropertyName("size")]
    public string Size { get; set; }
}

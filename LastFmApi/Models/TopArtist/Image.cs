using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.TopArtist;

public class Image
{
    [JsonProperty("size")]
    [JsonPropertyName("size")]
    public string Size { get; set; }

    [JsonProperty("#text")]
    [JsonPropertyName("#text")]
    public string Text { get; set; }
}

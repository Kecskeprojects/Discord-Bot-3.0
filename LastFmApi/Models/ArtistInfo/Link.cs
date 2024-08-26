using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.ArtistInfo;

public class Link
{
    [JsonProperty("#text")]
    [JsonPropertyName("#text")]
    public string Text { get; set; }

    [JsonProperty("rel")]
    [JsonPropertyName("rel")]
    public string Rel { get; set; }

    [JsonProperty("href")]
    [JsonPropertyName("href")]
    public string Href { get; set; }
}

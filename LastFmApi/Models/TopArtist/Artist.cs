using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.TopArtist;

public class Artist
{
    [JsonProperty("streamable")]
    [JsonPropertyName("streamable")]
    public string Streamable { get; set; }

    [JsonProperty("image")]
    [JsonPropertyName("image")]
    public List<Image> Image { get; set; }

    [JsonProperty("mbid")]
    [JsonPropertyName("mbid")]
    public string Mbid { get; set; }

    [JsonProperty("url")]
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonProperty("playcount")]
    [JsonPropertyName("playcount")]
    public string PlayCount { get; set; }

    [JsonProperty("@attr")]
    [JsonPropertyName("@attr")]
    public Attr Attr { get; set; }

    [JsonProperty("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

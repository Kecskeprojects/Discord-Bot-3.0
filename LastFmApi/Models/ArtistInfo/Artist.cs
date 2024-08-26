using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.ArtistInfo;

public class Artist
{
    [JsonProperty("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonProperty("mbid")]
    [JsonPropertyName("mbid")]
    public string Mbid { get; set; }

    [JsonProperty("url")]
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonProperty("image")]
    [JsonPropertyName("image")]
    public List<Image> Image { get; set; }

    [JsonProperty("streamable")]
    [JsonPropertyName("streamable")]
    public string Streamable { get; set; }

    [JsonProperty("ontour")]
    [JsonPropertyName("ontour")]
    public string Ontour { get; set; }

    [JsonProperty("stats")]
    [JsonPropertyName("stats")]
    public Stats Stats { get; set; }

    [JsonProperty("similar")]
    [JsonPropertyName("similar")]
    public Similar Similar { get; set; }

    [JsonProperty("tags")]
    [JsonPropertyName("tags")]
    public Tags Tags { get; set; }

    [JsonProperty("bio")]
    [JsonPropertyName("bio")]
    public Bio Bio { get; set; }
}

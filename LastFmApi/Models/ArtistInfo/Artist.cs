using System.Text.Json.Serialization;

namespace LastFmApi.Models.ArtistInfo;

public class Artist
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("mbid")]
    public string Mbid { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("image")]
    public List<Image> Image { get; set; }

    [JsonPropertyName("streamable")]
    public string Streamable { get; set; }

    [JsonPropertyName("ontour")]
    public string Ontour { get; set; }

    [JsonPropertyName("stats")]
    public Stats Stats { get; set; }

    [JsonPropertyName("similar")]
    public Similar Similar { get; set; }

    [JsonPropertyName("tags")]
    public Tags Tags { get; set; }

    [JsonPropertyName("bio")]
    public Bio Bio { get; set; }
}

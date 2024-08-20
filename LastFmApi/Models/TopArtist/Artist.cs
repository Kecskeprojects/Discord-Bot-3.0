using System.Text.Json.Serialization;

namespace LastFmApi.Models.TopArtist;

public class Artist
{
    [JsonPropertyName("streamable")]
    public string Streamable { get; set; }

    [JsonPropertyName("image")]
    public List<Image> Image { get; set; }

    [JsonPropertyName("mbid")]
    public string Mbid { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("playcount")]
    public string PlayCount { get; set; }

    [JsonPropertyName("@attr")]
    public Attr Attr { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}

using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.TopTrack;

public class Track
{
    [JsonProperty("streamable")]
    [JsonPropertyName("streamable")]
    public Streamable Streamable { get; set; }

    [JsonProperty("mbid")]
    [JsonPropertyName("mbid")]
    public string Mbid { get; set; }

    [JsonProperty("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonProperty("image")]
    [JsonPropertyName("image")]
    public List<Image> Image { get; set; }

    [JsonProperty("artist")]
    [JsonPropertyName("artist")]
    public Artist Artist { get; set; }

    [JsonProperty("url")]
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonProperty("duration")]
    [JsonPropertyName("duration")]
    public string Duration { get; set; }

    [JsonProperty("@attr")]
    [JsonPropertyName("@attr")]
    public Attr Attr { get; set; }

    [JsonProperty("playcount")]
    [JsonPropertyName("playcount")]
    public string PlayCount { get; set; }
}

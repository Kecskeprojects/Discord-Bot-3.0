using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.Recent;

public class Track
{
    [JsonProperty("artist")]
    [JsonPropertyName("artist")]
    public Artist Artist { get; set; }

    [JsonProperty("streamable")]
    [JsonPropertyName("streamable")]
    public string Streamable { get; set; }

    [JsonProperty("image")]
    [JsonPropertyName("image")]
    public List<Image> Image { get; set; }

    [JsonProperty("mbid")]
    [JsonPropertyName("mbid")]
    public string Mbid { get; set; }

    [JsonProperty("album")]
    [JsonPropertyName("album")]
    public Album Album { get; set; }

    [JsonProperty("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonProperty("@attr")]
    [JsonPropertyName("@attr")]
    public Attr Attr { get; set; }

    [JsonProperty("url")]
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonProperty("date")]
    [JsonPropertyName("date")]
    public Date Date { get; set; }
}

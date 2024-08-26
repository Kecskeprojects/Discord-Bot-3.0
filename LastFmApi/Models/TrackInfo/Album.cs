using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.TrackInfo;

public class Album
{
    [JsonProperty("artist")]
    [JsonPropertyName("artist")]
    public string Artist { get; set; }

    [JsonProperty("title")]
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonProperty("mbid")]
    [JsonPropertyName("mbid")]
    public string Mbid { get; set; }

    [JsonProperty("url")]
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonProperty("image")]
    [JsonPropertyName("image")]
    public List<Image> Image { get; set; }

    [JsonProperty("@attr")]
    [JsonPropertyName("@attr")]
    public Attr Attr { get; set; }
}

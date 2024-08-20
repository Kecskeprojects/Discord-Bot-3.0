using System.Text.Json.Serialization;

namespace LastFmApi.Models.TrackInfo;

public class Album
{
    [JsonPropertyName("artist")]
    public string Artist { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("mbid")]
    public string Mbid { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("image")]
    public List<Image> Image { get; set; }

    [JsonPropertyName("@attr")]
    public Attr Attr { get; set; }
}

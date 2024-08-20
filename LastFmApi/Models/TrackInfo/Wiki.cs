using System.Text.Json.Serialization;

namespace LastFmApi.Models.TrackInfo;
public class Wiki
{
    [JsonPropertyName("published")]
    public string Published { get; set; }

    [JsonPropertyName("summary")]
    public string Summary { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }
}

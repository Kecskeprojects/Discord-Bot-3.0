using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.TrackInfo;
public class Wiki
{
    [JsonProperty("published")]
    [JsonPropertyName("published")]
    public string Published { get; set; }

    [JsonProperty("summary")]
    [JsonPropertyName("summary")]
    public string Summary { get; set; }

    [JsonProperty("content")]
    [JsonPropertyName("content")]
    public string Content { get; set; }
}

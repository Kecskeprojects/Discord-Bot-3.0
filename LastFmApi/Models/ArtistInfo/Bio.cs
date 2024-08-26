using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.ArtistInfo;

public class Bio
{
    [JsonProperty("links")]
    [JsonPropertyName("links")]
    public Links Links { get; set; }

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

using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.ArtistInfo;

public class Tag
{
    [JsonProperty("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonProperty("url")]
    [JsonPropertyName("url")]
    public string Url { get; set; }
}

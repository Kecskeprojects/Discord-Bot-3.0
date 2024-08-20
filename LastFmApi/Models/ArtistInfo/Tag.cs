using System.Text.Json.Serialization;

namespace LastFmApi.Models.ArtistInfo;

public class Tag
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
}

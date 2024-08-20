using System.Text.Json.Serialization;

namespace LastFmApi.Models.TopAlbum;

public class Artist
{
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("mbid")]
    public string Mbid { get; set; }
}

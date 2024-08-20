using System.Text.Json.Serialization;

namespace LastFmApi.Models.Recent;

public class Artist
{
    [JsonPropertyName("mbid")]
    public string Mbid { get; set; }

    [JsonPropertyName("#text")]
    public string Text { get; set; }
}

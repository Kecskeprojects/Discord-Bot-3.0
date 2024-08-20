using System.Text.Json.Serialization;

namespace LastFmApi.Models.TopTrack;

public class Streamable
{
    [JsonPropertyName("fulltrack")]
    public string FullTrack { get; set; }

    [JsonPropertyName("#text")]
    public string Text { get; set; }
}

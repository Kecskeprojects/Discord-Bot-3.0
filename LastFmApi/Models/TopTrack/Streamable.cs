using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.TopTrack;

public class Streamable
{
    [JsonProperty("fulltrack")]
    [JsonPropertyName("fulltrack")]
    public string FullTrack { get; set; }

    [JsonProperty("#text")]
    [JsonPropertyName("#text")]
    public string Text { get; set; }
}

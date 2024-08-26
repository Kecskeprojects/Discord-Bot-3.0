using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.TrackInfo;

public class Streamable
{
    [JsonProperty("#text")]
    [JsonPropertyName("#text")]
    public string Text { get; set; }

    [JsonProperty("fulltrack")]
    [JsonPropertyName("fulltrack")]
    public string Fulltrack { get; set; }
}

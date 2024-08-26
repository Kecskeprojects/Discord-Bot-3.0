using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.Recent;

public class Date
{
    [JsonProperty("uts")]
    [JsonPropertyName("uts")]
    public string Uts { get; set; }

    [JsonProperty("#text")]
    [JsonPropertyName("#text")]
    public string Text { get; set; }
}

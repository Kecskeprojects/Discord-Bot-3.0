using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.TrackInfo;

public class Attr
{
    [JsonProperty("position")]
    [JsonPropertyName("position")]
    public string Position { get; set; }
}

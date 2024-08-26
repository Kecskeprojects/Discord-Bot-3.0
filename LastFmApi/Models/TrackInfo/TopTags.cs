using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.TrackInfo;

public class Toptags
{
    [JsonProperty("tag")]
    [JsonPropertyName("tag")]
    public List<Tag> Tag { get; set; }
}

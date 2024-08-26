using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.ArtistInfo;

public class Tags
{
    [JsonProperty("tag")]
    [JsonPropertyName("tag")]
    public List<Tag> Tag { get; set; }
}

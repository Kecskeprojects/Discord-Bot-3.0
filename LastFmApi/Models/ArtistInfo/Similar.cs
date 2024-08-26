using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.ArtistInfo;

public class Similar
{
    [JsonProperty("artist")]
    [JsonPropertyName("artist")]
    public List<Artist> Artist { get; set; }
}

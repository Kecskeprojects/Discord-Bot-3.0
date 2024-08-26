using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.ArtistInfo;

public class Links
{
    [JsonProperty("link")]
    [JsonPropertyName("link")]
    public Link Link { get; set; }
}

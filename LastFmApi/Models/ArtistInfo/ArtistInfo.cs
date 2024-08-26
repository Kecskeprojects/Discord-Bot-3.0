using LastFmApi.Communication;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.ArtistInfo;

public class ArtistInfo : BaseResponse
{
    [JsonProperty("artist")]
    [JsonPropertyName("artist")]
    public Artist Artist { get; set; }
}

using LastFmApi.Communication;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.TopTrack;

public class TopTrack : BaseResponse
{
    [JsonProperty("toptracks")]
    [JsonPropertyName("toptracks")]
    public Toptracks TopTracks { get; set; }
}

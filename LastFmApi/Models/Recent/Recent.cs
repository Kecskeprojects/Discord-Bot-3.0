using LastFmApi.Communication;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.Recent;

public class Recent : BaseResponse
{
    [JsonProperty("recenttracks")]
    [JsonPropertyName("recenttracks")]
    public Recenttracks RecentTracks { get; set; }
}

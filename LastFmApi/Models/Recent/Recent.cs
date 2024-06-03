using LastFmApi.Communication;
using Newtonsoft.Json;

namespace LastFmApi.Models.Recent;

public class Recent : BaseResponse
{
    [JsonProperty("recenttracks")]
    public Recenttracks RecentTracks { get; set; }
}

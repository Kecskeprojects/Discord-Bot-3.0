using LastFmApi.Communication;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.Recent;

public class Recent : BaseResponse
{
    [JsonPropertyName("recenttracks")]
    public Recenttracks RecentTracks { get; set; }
}

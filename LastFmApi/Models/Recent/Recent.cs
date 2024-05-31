using Newtonsoft.Json;

namespace LastFmApi.Models.Recent;

public class Recent
{
    [JsonProperty("recenttracks")]
    public Recenttracks RecentTracks { get; set; }
}

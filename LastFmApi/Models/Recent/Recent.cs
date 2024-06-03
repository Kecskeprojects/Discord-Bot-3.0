using Newtonsoft.Json;

namespace LastFmApi.Models.Recent;

public class Recent
{
    [JsonProperty("recenttracks")]
    public Recenttracks RecentTracks { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("message")]
    public int? Error { get; set; }
}

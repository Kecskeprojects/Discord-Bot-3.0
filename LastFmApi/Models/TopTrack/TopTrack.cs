using Newtonsoft.Json;

namespace LastFmApi.Models.TopTrack;

public class TopTrack
{
    [JsonProperty("toptracks")]
    public Toptracks TopTracks { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("message")]
    public int? Error { get; set; }
}

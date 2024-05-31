using Newtonsoft.Json;

namespace LastFmApi.Models.TopTrack;

public class TopTrack
{
    [JsonProperty("toptracks")]
    public Toptracks TopTracks { get; set; }
}

using Newtonsoft.Json;

namespace LastFmApi.Models.ArtistInfo;
public class Stats
{
    [JsonProperty("listeners")]
    public string Listeners { get; set; }

    [JsonProperty("playcount")]
    public string Playcount { get; set; }

    [JsonProperty("userplaycount")]
    public string Userplaycount { get; set; }
}

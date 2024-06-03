using Newtonsoft.Json;

namespace LastFmApi.Models.TrackInfo;

public class TrackInfo
{
    [JsonProperty("track")]
    public Track Track { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("message")]
    public int? Error { get; set; }
}

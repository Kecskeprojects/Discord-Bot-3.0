using Newtonsoft.Json;

namespace LastFmApi.Models.ArtistInfo;

public class ArtistInfo
{
    [JsonProperty("artist")]
    public Artist Artist { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("message")]
    public int? Error { get; set; }
}

using Newtonsoft.Json;

namespace LastFmApi.Models.TopArtist;

public class TopArtist
{
    [JsonProperty("topartists")]
    public Topartists TopArtists { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("message")]
    public int? Error { get; set; }
}

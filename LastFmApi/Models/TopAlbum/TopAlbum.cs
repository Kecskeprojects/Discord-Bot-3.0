using Newtonsoft.Json;

namespace LastFmApi.Models.TopAlbum;

public class TopAlbum
{
    [JsonProperty("topalbums")]
    public Topalbums TopAlbums { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("message")]
    public int? Error { get; set; }
}

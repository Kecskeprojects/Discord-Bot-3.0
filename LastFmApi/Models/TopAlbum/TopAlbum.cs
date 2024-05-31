using Newtonsoft.Json;

namespace LastFmApi.Models.TopAlbum;

public class TopAlbum
{
    [JsonProperty("topalbums")]
    public Topalbums TopAlbums { get; set; }
}

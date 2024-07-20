using Newtonsoft.Json;

namespace LastFmApi.Models.TopAlbum;
public class Topalbums
{
    [JsonProperty("album")]
    public List<Album> Album { get; set; }

    [JsonProperty("@attr")]
    public Attr Attr { get; set; }
}

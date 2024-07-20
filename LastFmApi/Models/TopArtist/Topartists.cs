using Newtonsoft.Json;

namespace LastFmApi.Models.TopArtist;

public class Topartists
{
    [JsonProperty("artist")]
    public List<Artist> Artist { get; set; }

    [JsonProperty("@attr")]
    public Attr Attr { get; set; }
}

using Newtonsoft.Json;

namespace LastFmApi.Models.ArtistInfo;

public class Similar
{
    [JsonProperty("artist")]
    public List<Artist> Artist { get; set; }
}

using Newtonsoft.Json;

namespace LastFmApi.Models.ArtistInfo;

public class Links
{
    [JsonProperty("link")]
    public Link Link { get; set; }
}

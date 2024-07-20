using LastFmApi.Communication;
using Newtonsoft.Json;

namespace LastFmApi.Models.TopArtist;
public class TopArtist : BaseResponse
{
    [JsonProperty("topartists")]
    public Topartists TopArtists { get; set; }
}

using LastFmApi.Communication;
using Newtonsoft.Json;

namespace LastFmApi.Models.ArtistInfo;

public class ArtistInfo : BaseResponse
{
    [JsonProperty("artist")]
    public Artist Artist { get; set; }
}

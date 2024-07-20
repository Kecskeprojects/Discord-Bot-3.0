using LastFmApi.Communication;
using Newtonsoft.Json;

namespace LastFmApi.Models.TopTrack;
public class TopTrack : BaseResponse
{
    [JsonProperty("toptracks")]
    public Toptracks TopTracks { get; set; }
}

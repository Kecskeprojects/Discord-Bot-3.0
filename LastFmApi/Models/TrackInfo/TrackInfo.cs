using LastFmApi.Communication;
using Newtonsoft.Json;

namespace LastFmApi.Models.TrackInfo;
public class TrackInfo : BaseResponse
{
    [JsonProperty("track")]
    public Track Track { get; set; }
}

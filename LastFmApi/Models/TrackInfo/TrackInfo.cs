using LastFmApi.Communication;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.TrackInfo;

public class TrackInfo : BaseResponse
{
    [JsonProperty("track")]
    [JsonPropertyName("track")]
    public Track Track { get; set; }
}

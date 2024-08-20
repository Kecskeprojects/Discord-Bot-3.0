using LastFmApi.Communication;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.TrackInfo;

public class TrackInfo : BaseResponse
{
    [JsonPropertyName("track")]
    public Track Track { get; set; }
}

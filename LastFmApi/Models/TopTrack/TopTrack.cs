using LastFmApi.Communication;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.TopTrack;

public class TopTrack : BaseResponse
{
    [JsonPropertyName("toptracks")]
    public Toptracks TopTracks { get; set; }
}

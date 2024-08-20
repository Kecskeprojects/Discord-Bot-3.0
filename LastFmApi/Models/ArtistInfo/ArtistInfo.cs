using LastFmApi.Communication;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.ArtistInfo;

public class ArtistInfo : BaseResponse
{
    [JsonPropertyName("artist")]
    public Artist Artist { get; set; }
}

using LastFmApi.Communication;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.TopArtist;

public class TopArtist : BaseResponse
{
    [JsonPropertyName("topartists")]
    public Topartists TopArtists { get; set; }
}

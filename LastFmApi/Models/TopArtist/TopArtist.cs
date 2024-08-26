using LastFmApi.Communication;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.TopArtist;

public class TopArtist : BaseResponse
{
    [JsonProperty("topartists")]
    [JsonPropertyName("topartists")]
    public Topartists TopArtists { get; set; }
}

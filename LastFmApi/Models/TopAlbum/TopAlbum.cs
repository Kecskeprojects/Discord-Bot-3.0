using LastFmApi.Communication;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.TopAlbum;

public class TopAlbum : BaseResponse
{
    [JsonProperty("topalbums")]
    [JsonPropertyName("topalbums")]
    public Topalbums TopAlbums { get; set; }
}

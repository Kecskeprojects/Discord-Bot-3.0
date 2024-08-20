using LastFmApi.Communication;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.TopAlbum;

public class TopAlbum : BaseResponse
{
    [JsonPropertyName("topalbums")]
    public Topalbums TopAlbums { get; set; }
}

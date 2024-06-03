using LastFmApi.Communication;
using Newtonsoft.Json;

namespace LastFmApi.Models.TopAlbum;

public class TopAlbum : BaseResponse
{
    [JsonProperty("topalbums")]
    public Topalbums TopAlbums { get; set; }
}

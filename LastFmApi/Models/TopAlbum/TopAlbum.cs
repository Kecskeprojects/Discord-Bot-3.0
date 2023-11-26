using System.Text.Json.Serialization;

namespace LastFmApi.Models.TopAlbum
{
    public class TopAlbum
    {
        [JsonPropertyName("topalbums")]
        public Topalbums TopAlbums { get; set; }
    }
}

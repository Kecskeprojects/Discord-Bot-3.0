using System.Text.Json.Serialization;

namespace LastFmApi.Models.TopAlbum
{
    public class Topalbums
    {
        [JsonPropertyName("album")]
        public List<Album> Album { get; set; }

        [JsonPropertyName("@attr")]
        public Attr Attr { get; set; }
    }
}

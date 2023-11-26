using System.Text.Json.Serialization;

namespace LastFmApi.Models.TopArtist
{
    public class TopArtist
    {
        [JsonPropertyName("topartists")]
        public Topartists TopArtists { get; set; }
    }
}

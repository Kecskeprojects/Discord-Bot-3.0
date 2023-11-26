using System.Text.Json.Serialization;

namespace LastFmApi.Models.ArtistInfo
{
    public class ArtistInfo
    {
        [JsonPropertyName("artist")]
        public Artist Artist { get; set; }
    }
}

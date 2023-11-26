using System.Text.Json.Serialization;

namespace LastFmApi.Models.ArtistInfo
{
    public class Stats
    {
        [JsonPropertyName("listeners")]
        public string Listeners { get; set; }

        [JsonPropertyName("playcount")]
        public string Playcount { get; set; }

        [JsonPropertyName("userplaycount")]
        public string Userplaycount { get; set; }
    }
}

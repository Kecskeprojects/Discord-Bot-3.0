using System.Text.Json.Serialization;

namespace LastFmApi.Models.TrackInfo
{
    public class Artist
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("mbid")]
        public string Mbid { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}

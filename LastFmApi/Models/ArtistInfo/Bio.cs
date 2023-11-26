using System.Text.Json.Serialization;

namespace LastFmApi.Models.ArtistInfo
{
    public class Bio
    {
        [JsonPropertyName("links")]
        public Links Links { get; set; }

        [JsonPropertyName("published")]
        public string Published { get; set; }

        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}

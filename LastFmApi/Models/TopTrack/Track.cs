using System.Text.Json.Serialization;

namespace LastFmApi.Models.TopTrack
{
    public class Track
    {
        [JsonPropertyName("streamable")]
        public Streamable Streamable { get; set; }

        [JsonPropertyName("mbid")]
        public string Mbid { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("image")]
        public List<Image> Image { get; set; }

        [JsonPropertyName("artist")]
        public Artist Artist { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("duration")]
        public string Duration { get; set; }


        [JsonPropertyName("@attr")]
        public Attr Attr { get; set; }

        [JsonPropertyName("playcount")]
        public string PlayCount { get; set; }
    }
}

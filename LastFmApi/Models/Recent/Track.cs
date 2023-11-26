using System.Text.Json.Serialization;

namespace LastFmApi.Models.Recent
{
    public class Track
    {
        [JsonPropertyName("artist")]
        public Artist Artist { get; set; }

        [JsonPropertyName("streamable")]
        public string Streamable { get; set; }

        [JsonPropertyName("image")]
        public List<Image> Image { get; set; }

        [JsonPropertyName("mbid")]
        public string Mbid { get; set; }

        [JsonPropertyName("album")]
        public Album Album { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("@attr")]
        public Attr Attr { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("date")]
        public Date Date { get; set; }
    }
}

using System.Text.Json.Serialization;

namespace LastFmApi.Models.ArtistInfo
{
    public class Link
    {
        [JsonPropertyName("#text")]
        public string Text { get; set; }

        [JsonPropertyName("rel")]
        public string Rel { get; set; }

        [JsonPropertyName("href")]
        public string Href { get; set; }
    }
}

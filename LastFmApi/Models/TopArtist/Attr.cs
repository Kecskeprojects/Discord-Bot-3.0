using System.Text.Json.Serialization;

namespace LastFmApi.Models.TopArtist
{
    public class Attr
    {
        [JsonPropertyName("rank")]
        public string Rank { get; set; }

        [JsonPropertyName("perPage")]
        public string PerPage { get; set; }

        [JsonPropertyName("totalPages")]
        public string TotalPages { get; set; }

        [JsonPropertyName("page")]
        public string Page { get; set; }

        [JsonPropertyName("total")]
        public string Total { get; set; }

        [JsonPropertyName("user")]
        public string User { get; set; }
    }
}

using System.Text.Json.Serialization;

namespace LastFmApi.Models.ArtistInfo
{
    public class Tags
    {
        [JsonPropertyName("tag")]
        public List<Tag> Tag { get; set; }
    }
}

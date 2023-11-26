using System.Text.Json.Serialization;

namespace LastFmApi.Models.TrackInfo
{
    public class Toptags
    {
        [JsonPropertyName("tag")]
        public List<Tag> Tag { get; set; }
    }
}

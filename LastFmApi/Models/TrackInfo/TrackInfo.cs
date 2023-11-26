using System.Text.Json.Serialization;

namespace LastFmApi.Models.TrackInfo
{
    public class TrackInfo
    {
        [JsonPropertyName("track")]
        public Track Track { get; set; }
    }
}

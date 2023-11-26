using System.Text.Json.Serialization;

namespace LastFmApi.Models.TopTrack
{
    public class TopTrack
    {
        [JsonPropertyName("toptracks")]
        public Toptracks TopTracks { get; set; }
    }
}

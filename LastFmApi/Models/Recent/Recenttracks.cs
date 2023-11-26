using System.Text.Json.Serialization;

namespace LastFmApi.Models.Recent
{
    public class Recenttracks
    {
        [JsonPropertyName("track")]
        public List<Track> Track { get; set; }

        [JsonPropertyName("@attr")]
        public Attr Attr { get; set; }
    }
}

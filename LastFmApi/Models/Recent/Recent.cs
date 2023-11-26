using System.Text.Json.Serialization;

namespace LastFmApi.Models.Recent
{
    public class Recent
    {
        [JsonPropertyName("recenttracks")]
        public Recenttracks RecentTracks { get; set; }
    }
}

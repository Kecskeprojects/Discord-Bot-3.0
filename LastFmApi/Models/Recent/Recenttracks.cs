using Newtonsoft.Json;

namespace LastFmApi.Models.Recent
{
    public class Recenttracks
    {
        [JsonProperty("track")]
        public List<Track> Track { get; set; }

        [JsonProperty("@attr")]
        public Attr Attr { get; set; }
    }
}

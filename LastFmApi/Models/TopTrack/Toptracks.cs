using Newtonsoft.Json;

namespace LastFmApi.Models.TopTrack
{
    public class Toptracks
    {
        [JsonProperty("track")]
        public List<Track> Track { get; set; }

        [JsonProperty("@attr")]
        public Attr Attr { get; set; }
    }
}

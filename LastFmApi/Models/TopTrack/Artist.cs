using Newtonsoft.Json;

namespace LastFmApi.Models.TopTrack
{
    public class Artist
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("mbid")]
        public string Mbid { get; set; }
    }
}

using Newtonsoft.Json;

namespace LastFmApi.Models.TrackInfo
{
    public class TrackInfo
    {
        [JsonProperty("track")]
        public Track Track { get; set; }
    }
}

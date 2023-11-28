using Newtonsoft.Json;

namespace LastFmApi.Models.TrackInfo
{
    public class Toptags
    {
        [JsonProperty("tag")]
        public List<Tag> Tag { get; set; }
    }
}

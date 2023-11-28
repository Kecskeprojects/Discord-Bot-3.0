using Newtonsoft.Json;

namespace LastFmApi.Models.TrackInfo
{
    public class Tag
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}

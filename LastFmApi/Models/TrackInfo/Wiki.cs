using Newtonsoft.Json;

namespace LastFmApi.Models.TrackInfo
{
    public class Wiki
    {
        [JsonProperty("published")]
        public string Published { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }
}

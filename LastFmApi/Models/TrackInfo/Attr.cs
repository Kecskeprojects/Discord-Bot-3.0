using Newtonsoft.Json;

namespace LastFmApi.Models.TrackInfo
{
    public class Attr
    {
        [JsonProperty("position")]
        public string Position { get; set; }
    }
}

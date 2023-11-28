using Newtonsoft.Json;

namespace LastFmApi.Models.ArtistInfo
{
    public class Tags
    {
        [JsonProperty("tag")]
        public List<Tag> Tag { get; set; }
    }
}

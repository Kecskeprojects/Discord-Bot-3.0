using Newtonsoft.Json;

namespace LastFmApi.Models.ArtistInfo
{
    public class ArtistInfo
    {
        [JsonProperty("artist")]
        public Artist Artist { get; set; }
    }
}

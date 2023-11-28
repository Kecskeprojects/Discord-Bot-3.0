using Newtonsoft.Json;

namespace LastFmApi.Models.TopArtist
{
    public class TopArtist
    {
        [JsonProperty("topartists")]
        public Topartists TopArtists { get; set; }
    }
}

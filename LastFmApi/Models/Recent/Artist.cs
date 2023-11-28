using Newtonsoft.Json;

namespace LastFmApi.Models.Recent
{
    public class Artist
    {
        [JsonProperty("mbid")]
        public string Mbid { get; set; }

        [JsonProperty("#text")]
        public string Text { get; set; }
    }
}

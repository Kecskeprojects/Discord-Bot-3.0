using System.Text.Json.Serialization;

namespace LastFmApi.Models.TrackInfo
{

    public class Streamable
    {
        [JsonPropertyName("#text")]
        public string Text { get; set; }

        [JsonPropertyName("fulltrack")]
        public string Fulltrack { get; set; }
    }
}

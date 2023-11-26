using System.Text.Json.Serialization;

namespace LastFmApi.Models.TrackInfo
{
    public class Attr
    {
        [JsonPropertyName("position")]
        public string Position { get; set; }
    }
}

using System.Text.Json.Serialization;

namespace LastFmApi.Models.Recent
{
    public class Date
    {
        [JsonPropertyName("uts")]
        public string Uts { get; set; }

        [JsonPropertyName("#text")]
        public string Text { get; set; }
    }
}

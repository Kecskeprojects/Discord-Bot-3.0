using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.MusicBrainz.ArtistLookup
{
    public class Url
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("resource")]
        public string Resource { get; set; }
    }
}

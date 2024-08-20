using System.Text.Json.Serialization;

namespace LastFmApi.Models.ArtistInfo;

public class Links
{
    [JsonPropertyName("link")]
    public Link Link { get; set; }
}

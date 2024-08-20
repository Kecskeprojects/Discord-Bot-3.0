using System.Text.Json.Serialization;

namespace LastFmApi.Models.ArtistInfo;

public class Similar
{
    [JsonPropertyName("artist")]
    public List<Artist> Artist { get; set; }
}

using System.Text.Json.Serialization;

namespace LastFmApi.Models.ArtistInfo;

public class Image
{
    [JsonPropertyName("#text")]
    public string Text { get; set; }

    [JsonPropertyName("size")]
    public string Size { get; set; }
}

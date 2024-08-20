using System.Text.Json.Serialization;

namespace LastFmApi.Models.TopArtist;

public class Image
{
    [JsonPropertyName("size")]
    public string Size { get; set; }

    [JsonPropertyName("#text")]
    public string Text { get; set; }
}

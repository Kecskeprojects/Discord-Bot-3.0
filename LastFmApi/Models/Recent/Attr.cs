using System.Text.Json.Serialization;

namespace LastFmApi.Models.Recent;

public class Attr
{
    [JsonPropertyName("nowplaying")]
    public string NowPlaying { get; set; }

    [JsonPropertyName("user")]
    public string User { get; set; }

    [JsonPropertyName("totalPages")]
    public string TotalPages { get; set; }

    [JsonPropertyName("page")]
    public string Page { get; set; }

    [JsonPropertyName("total")]
    public string Total { get; set; }

    [JsonPropertyName("perPage")]
    public string PerPage { get; set; }
}

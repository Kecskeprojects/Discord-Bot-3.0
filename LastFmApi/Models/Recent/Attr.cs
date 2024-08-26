using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.Recent;

public class Attr
{
    [JsonProperty("nowplaying")]
    [JsonPropertyName("nowplaying")]
    public string NowPlaying { get; set; }

    [JsonProperty("user")]
    [JsonPropertyName("user")]
    public string User { get; set; }

    [JsonProperty("totalPages")]
    [JsonPropertyName("totalPages")]
    public string TotalPages { get; set; }

    [JsonProperty("page")]
    [JsonPropertyName("page")]
    public string Page { get; set; }

    [JsonProperty("total")]
    [JsonPropertyName("total")]
    public string Total { get; set; }

    [JsonProperty("perPage")]
    [JsonPropertyName("perPage")]
    public string PerPage { get; set; }
}

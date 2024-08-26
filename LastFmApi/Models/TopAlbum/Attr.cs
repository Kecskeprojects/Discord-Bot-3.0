using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.TopAlbum;

public class Attr
{
    [JsonProperty("rank")]
    [JsonPropertyName("rank")]
    public string Rank { get; set; }

    [JsonProperty("perPage")]
    [JsonPropertyName("perPage")]
    public string PerPage { get; set; }

    [JsonProperty("totalPages")]
    [JsonPropertyName("totalPages")]
    public string TotalPages { get; set; }

    [JsonProperty("page")]
    [JsonPropertyName("page")]
    public string Page { get; set; }

    [JsonProperty("total")]
    [JsonPropertyName("total")]
    public string Total { get; set; }

    [JsonProperty("user")]
    [JsonPropertyName("user")]
    public string User { get; set; }
}

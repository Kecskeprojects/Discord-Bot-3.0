using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.TopTrack;

public class Attr
{
    [JsonProperty("rank")]
    [JsonPropertyName("rank")]
    public string Rank { get; set; }

    [JsonProperty("perPages")]
    [JsonPropertyName("perPages")]
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

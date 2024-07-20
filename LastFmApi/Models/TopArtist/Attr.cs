using Newtonsoft.Json;

namespace LastFmApi.Models.TopArtist;

public class Attr
{
    [JsonProperty("rank")]
    public string Rank { get; set; }

    [JsonProperty("perPage")]
    public string PerPage { get; set; }

    [JsonProperty("totalPages")]
    public string TotalPages { get; set; }

    [JsonProperty("page")]
    public string Page { get; set; }

    [JsonProperty("total")]
    public string Total { get; set; }

    [JsonProperty("user")]
    public string User { get; set; }
}

using Newtonsoft.Json;

namespace LastFmApi.Models.Recent;

public class Attr
{
    [JsonProperty("nowplaying")]
    public string NowPlaying { get; set; }

    [JsonProperty("user")]
    public string User { get; set; }

    [JsonProperty("totalPages")]
    public string TotalPages { get; set; }

    [JsonProperty("page")]
    public string Page { get; set; }

    [JsonProperty("total")]
    public string Total { get; set; }

    [JsonProperty("perPage")]
    public string PerPage { get; set; }
}

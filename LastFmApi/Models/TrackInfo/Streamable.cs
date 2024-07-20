using Newtonsoft.Json;

namespace LastFmApi.Models.TrackInfo;

public class Streamable
{
    [JsonProperty("#text")]
    public string Text { get; set; }

    [JsonProperty("fulltrack")]
    public string Fulltrack { get; set; }
}

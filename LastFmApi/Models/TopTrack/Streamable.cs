using Newtonsoft.Json;

namespace LastFmApi.Models.TopTrack;

public class Streamable
{
    [JsonProperty("fulltrack")]
    public string FullTrack { get; set; }

    [JsonProperty("#text")]
    public string Text { get; set; }
}

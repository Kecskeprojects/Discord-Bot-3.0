using Newtonsoft.Json;

namespace LastFmApi.Models.TrackInfo;

public class Image
{
    [JsonProperty("#text")]
    public string Text { get; set; }

    [JsonProperty("size")]
    public string Size { get; set; }
}

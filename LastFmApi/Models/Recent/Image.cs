using Newtonsoft.Json;

namespace LastFmApi.Models.Recent;
public class Image
{
    [JsonProperty("size")]
    public string Size { get; set; }

    [JsonProperty("#text")]
    public string Text { get; set; }
}

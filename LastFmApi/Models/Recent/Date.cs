using Newtonsoft.Json;

namespace LastFmApi.Models.Recent;
public class Date
{
    [JsonProperty("uts")]
    public string Uts { get; set; }

    [JsonProperty("#text")]
    public string Text { get; set; }
}

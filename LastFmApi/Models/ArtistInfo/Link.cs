using Newtonsoft.Json;

namespace LastFmApi.Models.ArtistInfo;
public class Link
{
    [JsonProperty("#text")]
    public string Text { get; set; }

    [JsonProperty("rel")]
    public string Rel { get; set; }

    [JsonProperty("href")]
    public string Href { get; set; }
}

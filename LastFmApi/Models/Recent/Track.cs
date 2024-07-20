using Newtonsoft.Json;

namespace LastFmApi.Models.Recent;
public class Track
{
    [JsonProperty("artist")]
    public Artist Artist { get; set; }

    [JsonProperty("streamable")]
    public string Streamable { get; set; }

    [JsonProperty("image")]
    public List<Image> Image { get; set; }

    [JsonProperty("mbid")]
    public string Mbid { get; set; }

    [JsonProperty("album")]
    public Album Album { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("@attr")]
    public Attr Attr { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("date")]
    public Date Date { get; set; }
}

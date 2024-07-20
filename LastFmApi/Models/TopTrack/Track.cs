using Newtonsoft.Json;

namespace LastFmApi.Models.TopTrack;
public class Track
{
    [JsonProperty("streamable")]
    public Streamable Streamable { get; set; }

    [JsonProperty("mbid")]
    public string Mbid { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("image")]
    public List<Image> Image { get; set; }

    [JsonProperty("artist")]
    public Artist Artist { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("duration")]
    public string Duration { get; set; }

    [JsonProperty("@attr")]
    public Attr Attr { get; set; }

    [JsonProperty("playcount")]
    public string PlayCount { get; set; }
}

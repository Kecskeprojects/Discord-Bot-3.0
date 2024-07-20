using Newtonsoft.Json;

namespace LastFmApi.Models.TopAlbum;
public class Album
{
    [JsonProperty("artist")]
    public Artist Artist { get; set; }

    [JsonProperty("image")]
    public List<Image> Image { get; set; }

    [JsonProperty("mbid")]
    public string Mbid { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("playcount")]
    public string PlayCount { get; set; }

    [JsonProperty("@attr")]
    public Attr Attr { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }
}

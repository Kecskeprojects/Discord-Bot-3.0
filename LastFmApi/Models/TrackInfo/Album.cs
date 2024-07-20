using Newtonsoft.Json;

namespace LastFmApi.Models.TrackInfo;

public class Album
{
    [JsonProperty("artist")]
    public string Artist { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("mbid")]
    public string Mbid { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("image")]
    public List<Image> Image { get; set; }

    [JsonProperty("@attr")]
    public Attr Attr { get; set; }
}

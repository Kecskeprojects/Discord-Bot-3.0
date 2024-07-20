using Newtonsoft.Json;

namespace LastFmApi.Models.ArtistInfo;

public class Artist
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("mbid")]
    public string Mbid { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("image")]
    public List<Image> Image { get; set; }

    [JsonProperty("streamable")]
    public string Streamable { get; set; }

    [JsonProperty("ontour")]
    public string Ontour { get; set; }

    [JsonProperty("stats")]
    public Stats Stats { get; set; }

    [JsonProperty("similar")]
    public Similar Similar { get; set; }

    [JsonProperty("tags")]
    public Tags Tags { get; set; }

    [JsonProperty("bio")]
    public Bio Bio { get; set; }
}

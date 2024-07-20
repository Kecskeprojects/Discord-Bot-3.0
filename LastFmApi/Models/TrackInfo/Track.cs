using Newtonsoft.Json;

namespace LastFmApi.Models.TrackInfo;
public class Track
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("mbid")]
    public string Mbid { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("duration")]
    public string Duration { get; set; }

    [JsonProperty("streamable")]
    public Streamable Streamable { get; set; }

    [JsonProperty("listeners")]
    public string Listeners { get; set; }

    [JsonProperty("playcount")]
    public string Playcount { get; set; }

    [JsonProperty("artist")]
    public Artist Artist { get; set; }

    [JsonProperty("album")]
    public Album Album { get; set; }

    [JsonProperty("userplaycount")]
    public string Userplaycount { get; set; }

    [JsonProperty("userloved")]
    public string Userloved { get; set; }

    [JsonProperty("toptags")]
    public Toptags Toptags { get; set; }

    [JsonProperty("wiki")]
    public Wiki Wiki { get; set; }
}

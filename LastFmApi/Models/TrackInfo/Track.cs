using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Models.TrackInfo;

public class Track
{
    [JsonProperty("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonProperty("mbid")]
    [JsonPropertyName("mbid")]
    public string Mbid { get; set; }

    [JsonProperty("url")]
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonProperty("duration")]
    [JsonPropertyName("duration")]
    public string Duration { get; set; }

    [JsonProperty("streamable")]
    [JsonPropertyName("streamable")]
    public Streamable Streamable { get; set; }

    [JsonProperty("listeners")]
    [JsonPropertyName("listeners")]
    public string Listeners { get; set; }

    [JsonProperty("playcount")]
    [JsonPropertyName("playcount")]
    public string Playcount { get; set; }

    [JsonProperty("artist")]
    [JsonPropertyName("artist")]
    public Artist Artist { get; set; }

    [JsonProperty("album")]
    [JsonPropertyName("album")]
    public Album Album { get; set; }

    [JsonProperty("userplaycount")]
    [JsonPropertyName("userplaycount")]
    public string Userplaycount { get; set; }

    [JsonProperty("userloved")]
    [JsonPropertyName("userloved")]
    public string Userloved { get; set; }

    [JsonProperty("toptags")]
    [JsonPropertyName("toptags")]
    public Toptags Toptags { get; set; }

    [JsonProperty("wiki")]
    [JsonPropertyName("wiki")]
    public Wiki Wiki { get; set; }
}

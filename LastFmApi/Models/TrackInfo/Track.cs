using System.Text.Json.Serialization;

namespace LastFmApi.Models.TrackInfo;

public class Track
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("mbid")]
    public string Mbid { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("duration")]
    public string Duration { get; set; }

    [JsonPropertyName("streamable")]
    public Streamable Streamable { get; set; }

    [JsonPropertyName("listeners")]
    public string Listeners { get; set; }

    [JsonPropertyName("playcount")]
    public string Playcount { get; set; }

    [JsonPropertyName("artist")]
    public Artist Artist { get; set; }

    [JsonPropertyName("album")]
    public Album Album { get; set; }

    [JsonPropertyName("userplaycount")]
    public string Userplaycount { get; set; }

    [JsonPropertyName("userloved")]
    public string Userloved { get; set; }

    [JsonPropertyName("toptags")]
    public Toptags Toptags { get; set; }

    [JsonPropertyName("wiki")]
    public Wiki Wiki { get; set; }
}

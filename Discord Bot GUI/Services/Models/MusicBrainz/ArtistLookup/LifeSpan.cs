using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.MusicBrainz.ArtistLookup;

public class LifeSpan
{
    [JsonPropertyName("begin")]
    public string Begin { get; set; }

    [JsonPropertyName("ended")]
    public bool Ended { get; set; }

    [JsonPropertyName("end")]
    public object End { get; set; }
}

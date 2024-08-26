using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.MusicBrainz.ArtistLookup;

public class LifeSpan
{
    [JsonProperty("begin")]
    [JsonPropertyName("begin")]
    public string Begin { get; set; }

    [JsonProperty("ended")]
    [JsonPropertyName("ended")]
    public bool Ended { get; set; }

    [JsonProperty("end")]
    [JsonPropertyName("end")]
    public object End { get; set; }
}

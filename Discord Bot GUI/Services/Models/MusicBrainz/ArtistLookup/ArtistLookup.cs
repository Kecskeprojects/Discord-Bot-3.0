using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.MusicBrainz.ArtistLookup;

public class ArtistLookup
{
    [JsonPropertyName("area")]
    public Area Area { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("begin_area")]
    public BeginArea BeginArea { get; set; }

    [JsonPropertyName("sort-name")]
    public string SortName { get; set; }

    [JsonPropertyName("life-span")]
    public LifeSpan LifeSpan { get; set; }

    [JsonPropertyName("country")]
    public string Country { get; set; }

    [JsonPropertyName("type-id")]
    public string TypeId { get; set; }

    [JsonPropertyName("gender-id")]
    public object GenderId { get; set; }

    [JsonPropertyName("ipis")]
    public List<object> Ipis { get; set; }

    [JsonPropertyName("begin-area")]
    public BeginArea BeginArea2 { get; set; }

    [JsonPropertyName("end-area")]
    public object EndArea { get; set; }

    [JsonPropertyName("isnis")]
    public List<string> Isnis { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("gender")]
    public object Gender { get; set; }

    [JsonPropertyName("disambiguation")]
    public string Disambiguation { get; set; }

    [JsonPropertyName("relations")]
    public List<Relation> Relations { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("end_area")]
    public object EndArea2 { get; set; }
}

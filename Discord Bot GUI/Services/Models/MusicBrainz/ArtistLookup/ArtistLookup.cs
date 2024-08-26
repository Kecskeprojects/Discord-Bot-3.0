using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.MusicBrainz.ArtistLookup;

public class ArtistLookup
{
    [JsonProperty("area")]
    [JsonPropertyName("area")]
    public Area Area { get; set; }

    [JsonProperty("type")]
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonProperty("begin_area")]
    [JsonPropertyName("begin_area")]
    public BeginArea BeginArea { get; set; }

    [JsonProperty("sort-name")]
    [JsonPropertyName("sort-name")]
    public string SortName { get; set; }

    [JsonProperty("life-span")]
    [JsonPropertyName("life-span")]
    public LifeSpan LifeSpan { get; set; }

    [JsonProperty("country")]
    [JsonPropertyName("country")]
    public string Country { get; set; }

    [JsonProperty("type-id")]
    [JsonPropertyName("type-id")]
    public string TypeId { get; set; }

    [JsonProperty("gender-id")]
    [JsonPropertyName("gender-id")]
    public object GenderId { get; set; }

    [JsonProperty("ipis")]
    [JsonPropertyName("ipis")]
    public List<object> Ipis { get; set; }

    [JsonProperty("begin-area")]
    [JsonPropertyName("begin-area")]
    public BeginArea BeginArea2 { get; set; }

    [JsonProperty("end-area")]
    [JsonPropertyName("end-area")]
    public object EndArea { get; set; }

    [JsonProperty("isnis")]
    [JsonPropertyName("isnis")]
    public List<string> Isnis { get; set; }

    [JsonProperty("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonProperty("gender")]
    [JsonPropertyName("gender")]
    public object Gender { get; set; }

    [JsonProperty("disambiguation")]
    [JsonPropertyName("disambiguation")]
    public string Disambiguation { get; set; }

    [JsonProperty("relations")]
    [JsonPropertyName("relations")]
    public List<Relation> Relations { get; set; }

    [JsonProperty("id")]
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonProperty("end_area")]
    [JsonPropertyName("end_area")]
    public object EndArea2 { get; set; }
}

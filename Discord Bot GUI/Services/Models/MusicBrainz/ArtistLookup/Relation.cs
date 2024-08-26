using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.MusicBrainz.ArtistLookup;

public class Relation
{
    [JsonProperty("attribute-ids")]
    [JsonPropertyName("attribute-ids")]
    public object AttributeIds { get; set; }

    [JsonProperty("end")]
    [JsonPropertyName("end")]
    public string End { get; set; }

    [JsonProperty("url")]
    [JsonPropertyName("url")]
    public Url Url { get; set; }

    [JsonProperty("type")]
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonProperty("target-type")]
    [JsonPropertyName("target-type")]
    public string TargetType { get; set; }

    [JsonProperty("begin")]
    [JsonPropertyName("begin")]
    public object Begin { get; set; }

    [JsonProperty("target-credit")]
    [JsonPropertyName("target-credit")]
    public string TargetCredit { get; set; }

    [JsonProperty("source-credit")]
    [JsonPropertyName("source-credit")]
    public string SourceCredit { get; set; }

    [JsonProperty("ended")]
    [JsonPropertyName("ended")]
    public bool Ended { get; set; }

    [JsonProperty("attributes")]
    [JsonPropertyName("attributes")]
    public List<object> Attributes { get; set; }

    [JsonProperty("attribute-values")]
    [JsonPropertyName("attribute-values")]
    public object AttributeValues { get; set; }

    [JsonProperty("direction")]
    [JsonPropertyName("direction")]
    public string Direction { get; set; }

    [JsonProperty("type-id")]
    [JsonPropertyName("type-id")]
    public string TypeId { get; set; }
}

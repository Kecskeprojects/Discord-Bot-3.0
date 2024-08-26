using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.MusicBrainz.ArtistLookup;

public class BeginArea
{
    [JsonProperty("id")]
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonProperty("sort-name")]
    [JsonPropertyName("sort-name")]
    public string SortName { get; set; }

    [JsonProperty("iso-3166-2-codes")]
    [JsonPropertyName("iso-3166-2-codes")]
    public List<string> Iso31662Codes { get; set; }

    [JsonProperty("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonProperty("disambiguation")]
    [JsonPropertyName("disambiguation")]
    public string Disambiguation { get; set; }

    [JsonProperty("type")]
    [JsonPropertyName("type")]
    public object Type { get; set; }

    [JsonProperty("type-id")]
    [JsonPropertyName("type-id")]
    public object TypeId { get; set; }
}

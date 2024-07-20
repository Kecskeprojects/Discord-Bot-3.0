using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.MusicBrainz.ArtistLookup;

public class BeginArea
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("sort-name")]
    public string SortName { get; set; }

    [JsonPropertyName("iso-3166-2-codes")]
    public List<string> Iso31662Codes { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("disambiguation")]
    public string Disambiguation { get; set; }

    [JsonPropertyName("type")]
    public object Type { get; set; }

    [JsonPropertyName("type-id")]
    public object TypeId { get; set; }
}

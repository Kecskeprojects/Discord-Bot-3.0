using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class Url2
{
    [JsonProperty("display_url")]
    [JsonPropertyName("display_url")]
    public string DisplayUrl { get; set; }

    [JsonProperty("expanded_url")]
    [JsonPropertyName("expanded_url")]
    public string ExpandedUrl { get; set; }

    [JsonProperty("url")]
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonProperty("indices")]
    [JsonPropertyName("indices")]
    public List<int> Indices { get; set; }
}

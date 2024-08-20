using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class Url2
{
    [JsonPropertyName("display_url")]
    public string DisplayUrl { get; set; }

    [JsonPropertyName("expanded_url")]
    public string ExpandedUrl { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("indices")]
    public List<int> Indices { get; set; }
}

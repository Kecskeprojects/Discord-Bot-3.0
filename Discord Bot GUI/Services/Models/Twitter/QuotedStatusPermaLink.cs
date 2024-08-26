using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class QuotedStatusPermalink
{
    [JsonProperty("url")]
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonProperty("expanded")]
    [JsonPropertyName("expanded")]
    public string Expanded { get; set; }

    [JsonProperty("display")]
    [JsonPropertyName("display")]
    public string Display { get; set; }
}

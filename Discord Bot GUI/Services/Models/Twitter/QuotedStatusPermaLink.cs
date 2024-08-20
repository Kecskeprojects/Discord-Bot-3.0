using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class QuotedStatusPermalink
{
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("expanded")]
    public string Expanded { get; set; }

    [JsonPropertyName("display")]
    public string Display { get; set; }
}

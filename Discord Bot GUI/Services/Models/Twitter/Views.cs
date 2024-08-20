using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class Views
{
    [JsonPropertyName("count")]
    public string Count { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }
}

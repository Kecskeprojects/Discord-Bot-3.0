using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class Views
{
    [JsonProperty("count")]
    [JsonPropertyName("count")]
    public string Count { get; set; }

    [JsonProperty("state")]
    [JsonPropertyName("state")]
    public string State { get; set; }
}

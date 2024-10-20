using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Instagram;
public class Root
{
    [JsonProperty("data")]
    [JsonPropertyName("data")]
    public Data Data { get; set; }

    [JsonProperty("extensions")]
    [JsonPropertyName("extensions")]
    public Extensions Extensions { get; set; }

    [JsonProperty("status")]
    [JsonPropertyName("status")]
    public string Status { get; set; }
}

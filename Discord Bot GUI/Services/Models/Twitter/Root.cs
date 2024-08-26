using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class Root
{
    [JsonProperty("data")]
    [JsonPropertyName("data")]
    public Data Data { get; set; }
}

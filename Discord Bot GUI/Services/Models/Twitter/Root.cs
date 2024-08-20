using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class Root
{
    [JsonPropertyName("data")]
    public Data Data { get; set; }
}

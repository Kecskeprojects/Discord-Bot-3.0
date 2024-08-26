using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class Face
{
    [JsonProperty("x")]
    [JsonPropertyName("x")]
    public int X { get; set; }

    [JsonProperty("y")]
    [JsonPropertyName("y")]
    public int Y { get; set; }

    [JsonProperty("h")]
    [JsonPropertyName("h")]
    public int H { get; set; }

    [JsonProperty("w")]
    [JsonPropertyName("w")]
    public int W { get; set; }
}

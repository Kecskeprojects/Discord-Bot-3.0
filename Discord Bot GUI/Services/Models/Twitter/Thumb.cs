using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class Thumb
{
    [JsonProperty("h")]
    [JsonPropertyName("h")]
    public int H { get; set; }

    [JsonProperty("w")]
    [JsonPropertyName("w")]
    public int W { get; set; }

    [JsonProperty("resize")]
    [JsonPropertyName("resize")]
    public string Resize { get; set; }
}

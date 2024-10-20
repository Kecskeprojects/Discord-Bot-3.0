using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.InstaLoader;

public class InstaLoaderBase
{
    [JsonProperty("instaloader")]
    [JsonPropertyName("instaloader")]
    public InstaLoader Instaloader { get; set; }

    [JsonProperty("node")]
    [JsonPropertyName("node")]
    public Node Node { get; set; }
}

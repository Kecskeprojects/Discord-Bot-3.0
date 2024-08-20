using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Instagram;

public class InstaLoaderBase
{
    [JsonPropertyName("instaloader")]
    public InstaLoader Instaloader { get; set; }

    [JsonPropertyName("node")]
    public Node Node { get; set; }
}

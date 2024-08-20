using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Instagram;

public class InstaLoader
{
    [JsonPropertyName("node_type")]
    public string NodeType { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; }
}

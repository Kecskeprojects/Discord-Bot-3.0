using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Instagram;

public class InstaLoader
{
    [JsonProperty("node_type")]
    [JsonPropertyName("node_type")]
    public string NodeType { get; set; }

    [JsonProperty("version")]
    [JsonPropertyName("version")]
    public string Version { get; set; }
}

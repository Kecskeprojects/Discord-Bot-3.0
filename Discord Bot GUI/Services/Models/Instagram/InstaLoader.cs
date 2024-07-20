using Newtonsoft.Json;

namespace Discord_Bot.Services.Models.Instagram;
public class InstaLoader
{
    [JsonProperty("node_type")]
    public string NodeType { get; set; }

    [JsonProperty("version")]
    public string Version { get; set; }
}

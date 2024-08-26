using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Instagram.Edge;

public class Edge
{
    [JsonProperty("node")]
    [JsonPropertyName("node")]
    public Node Node { get; set; }
}

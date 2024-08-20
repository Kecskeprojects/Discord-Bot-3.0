using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Instagram.Edge;

public class Edge
{
    [JsonPropertyName("node")]
    public Node Node { get; set; }
}

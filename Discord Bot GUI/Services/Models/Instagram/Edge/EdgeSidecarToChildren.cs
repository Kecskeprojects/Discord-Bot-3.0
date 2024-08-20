using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Instagram.Edge;

public class EdgeSidecarToChildren
{
    [JsonPropertyName("edges")]
    public List<Edge> Edges { get; set; }
}

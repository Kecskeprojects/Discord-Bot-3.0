using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Instagram.Edge;

public class EdgeMediaToTaggedUser
{
    [JsonPropertyName("edges")]
    public List<Edge> Edges { get; set; }
}

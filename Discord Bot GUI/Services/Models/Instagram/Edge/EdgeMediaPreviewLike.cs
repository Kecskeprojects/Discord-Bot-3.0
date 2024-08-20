using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Instagram.Edge;

public class EdgeMediaPreviewLike
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("edges")]
    public List<object> Edges { get; set; }
}

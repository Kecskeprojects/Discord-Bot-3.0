using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Instagram.Edge;

public class EdgeMediaPreviewLike
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("edges")]
    public List<object> Edges { get; set; }
}

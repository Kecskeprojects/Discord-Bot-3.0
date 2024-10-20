using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.InstaLoader.Edge;

public class EdgeMediaPreviewLike
{
    [JsonProperty("count")]
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonProperty("edges")]
    [JsonPropertyName("edges")]
    public List<object> Edges { get; set; }
}

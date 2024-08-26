using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Instagram.Edge;

public class EdgeMediaToCaption
{
    [JsonProperty("edges")]
    [JsonPropertyName("edges")]
    public List<Edge> Edges { get; set; }
}

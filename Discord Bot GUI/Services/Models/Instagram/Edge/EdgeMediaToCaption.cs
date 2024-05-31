using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Instagram.Edge;

public class EdgeMediaToCaption
{
    [JsonProperty("edges")]
    public List<Edge> Edges { get; set; }
}

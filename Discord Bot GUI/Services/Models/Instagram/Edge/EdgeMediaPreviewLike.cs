using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Instagram.Edge;

public class EdgeMediaPreviewLike
{
    [JsonProperty("count")]
    public int Count { get; set; }

    [JsonProperty("edges")]
    public List<object> Edges { get; set; }
}

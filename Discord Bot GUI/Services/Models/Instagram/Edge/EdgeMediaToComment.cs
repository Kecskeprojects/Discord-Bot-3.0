using Discord_Bot.Services.Models.Instagram.OtherSubClasses;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Instagram.Edge;
public class EdgeMediaToComment
{
    [JsonProperty("count")]
    public int Count { get; set; }

    [JsonProperty("edges")]
    public List<object> Edges { get; set; }

    [JsonProperty("page_info")]
    public PageInfo PageInfo { get; set; }
}

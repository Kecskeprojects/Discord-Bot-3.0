using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Instagram.Edge;
public class EdgeWebMediaToRelatedMedia
{
    [JsonProperty("edges")]
    public List<object> Edges { get; set; }
}

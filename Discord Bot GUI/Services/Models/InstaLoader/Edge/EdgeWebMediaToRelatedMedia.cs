using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.InstaLoader.Edge;

public class EdgeWebMediaToRelatedMedia
{
    [JsonProperty("edges")]
    [JsonPropertyName("edges")]
    public List<object> Edges { get; set; }
}

using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Instagram;
public class EdgeLikedBy
{
    [JsonProperty("count")]
    [JsonPropertyName("count")]
    public int Count { get; set; }
}

using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Instagram;
public class EdgeOwnerToTimelineMedia
{
    [JsonProperty("count")]
    [JsonPropertyName("count")]
    public int Count { get; set; }
}

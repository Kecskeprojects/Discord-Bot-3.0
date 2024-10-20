using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Instagram;
public class Extensions
{
    [JsonProperty("is_final")]
    [JsonPropertyName("is_final")]
    public bool IsFinal { get; set; }
}

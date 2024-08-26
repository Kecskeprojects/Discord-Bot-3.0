using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class AdditionalMediaInfo
{
    [JsonProperty("monetizable")]
    [JsonPropertyName("monetizable")]
    public bool Monetizable { get; set; }
}

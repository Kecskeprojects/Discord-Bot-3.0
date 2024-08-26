using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class ExtMediaAvailability
{
    [JsonProperty("status")]
    [JsonPropertyName("status")]
    public string Status { get; set; }
}

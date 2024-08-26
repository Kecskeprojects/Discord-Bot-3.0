using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class Features
{
    [JsonProperty("large")]
    [JsonPropertyName("large")]
    public Large Large { get; set; }

    [JsonProperty("medium")]
    [JsonPropertyName("medium")]
    public Medium Medium { get; set; }

    [JsonProperty("small")]
    [JsonPropertyName("small")]
    public Small Small { get; set; }

    [JsonProperty("orig")]
    [JsonPropertyName("orig")]
    public Orig Orig { get; set; }
}

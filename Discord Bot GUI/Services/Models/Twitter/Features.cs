using Newtonsoft.Json;

namespace Discord_Bot.Services.Models.Twitter;

public class Features
{
    [JsonProperty("large")]
    public Large Large { get; set; }

    [JsonProperty("medium")]
    public Medium Medium { get; set; }

    [JsonProperty("small")]
    public Small Small { get; set; }

    [JsonProperty("orig")]
    public Orig Orig { get; set; }
}

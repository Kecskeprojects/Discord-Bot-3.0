using Newtonsoft.Json;

namespace Discord_Bot.Services.Models.Twitter;

public class AdditionalMediaInfo
{
    [JsonProperty("monetizable")]
    public bool Monetizable { get; set; }
}

using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class AdditionalMediaInfo
{
    [JsonPropertyName("monetizable")]
    public bool Monetizable { get; set; }
}

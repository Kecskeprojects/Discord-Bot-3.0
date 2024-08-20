using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class ExtMediaAvailability
{
    [JsonPropertyName("status")]
    public string Status { get; set; }
}

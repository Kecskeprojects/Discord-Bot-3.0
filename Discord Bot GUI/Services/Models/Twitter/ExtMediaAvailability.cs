using Newtonsoft.Json;

namespace Discord_Bot.Services.Models.Twitter;
public class ExtMediaAvailability
{
    [JsonProperty("status")]
    public string Status { get; set; }
}

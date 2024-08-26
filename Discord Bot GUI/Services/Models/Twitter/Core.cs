using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class Core
{
    [JsonProperty("user_results")]
    [JsonPropertyName("user_results")]
    public UserResults UserResults { get; set; }
}

using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class Core
{
    [JsonPropertyName("user_results")]
    public UserResults UserResults { get; set; }
}

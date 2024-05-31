using Newtonsoft.Json;

namespace Discord_Bot.Services.Models.Twitter;

public class Core
{
    [JsonProperty("user_results")]
    public UserResults UserResults { get; set; }
}

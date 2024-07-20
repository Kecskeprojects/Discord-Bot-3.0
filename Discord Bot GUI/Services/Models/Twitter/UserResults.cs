using Newtonsoft.Json;

namespace Discord_Bot.Services.Models.Twitter;

public class UserResults
{
    [JsonProperty("result")]
    public Result Result { get; set; }
}

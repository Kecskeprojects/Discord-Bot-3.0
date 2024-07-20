using Newtonsoft.Json;

namespace Discord_Bot.Services.Models.Twitter;
public class Root
{
    [JsonProperty("data")]
    public Data Data { get; set; }
}

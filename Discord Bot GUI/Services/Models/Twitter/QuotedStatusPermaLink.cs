using Newtonsoft.Json;

namespace Discord_Bot.Services.Models.Twitter;

public class QuotedStatusPermalink
{
    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("expanded")]
    public string Expanded { get; set; }

    [JsonProperty("display")]
    public string Display { get; set; }
}

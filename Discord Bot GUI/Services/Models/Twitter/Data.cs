using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class Data
{
    [JsonProperty("tweetResult")]
    [JsonPropertyName("tweetResult")]
    public TweetResult TweetResult { get; set; }
}

using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class Data
{
    [JsonPropertyName("tweetResult")]
    public TweetResult TweetResult { get; set; }
}

using Newtonsoft.Json;

namespace Discord_Bot.Services.Models.Twitter
{
    public class Data
    {
        [JsonProperty("tweetResult")]
        public TweetResult TweetResult { get; set; }
    }
}

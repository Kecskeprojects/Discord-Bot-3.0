using Newtonsoft.Json;

namespace Discord_Bot.Services.Models.Twitter
{
    public class Views
    {
        [JsonProperty("count")]
        public string Count { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }
}

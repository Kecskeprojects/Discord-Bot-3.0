using Newtonsoft.Json;

namespace Discord_Bot.Services.Models.Instagram
{
    public class InstaLoaderBase
    {
        [JsonProperty("instaloader")]
        public InstaLoader Instaloader { get; set; }

        [JsonProperty("node")]
        public Node Node { get; set; }
    }
}

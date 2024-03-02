using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Twitter
{
    public class Entities
    {
        [JsonProperty("description")]
        public Description Description { get; set; }

        [JsonProperty("hashtags")]
        public List<object> Hashtags { get; set; }

        [JsonProperty("media")]
        public List<Medium> Media { get; set; }

        [JsonProperty("symbols")]
        public List<object> Symbols { get; set; }

        [JsonProperty("timestamps")]
        public List<object> Timestamps { get; set; }

        [JsonProperty("urls")]
        public List<object> Urls { get; set; }

        [JsonProperty("user_mentions")]
        public List<object> UserMentions { get; set; }
    }
}

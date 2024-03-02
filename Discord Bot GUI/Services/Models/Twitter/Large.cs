using Newtonsoft.Json;

namespace Discord_Bot.Services.Models.Twitter
{
    public class Large
    {
        [JsonProperty("h")]
        public int H { get; set; }

        [JsonProperty("w")]
        public int W { get; set; }

        [JsonProperty("resize")]
        public string Resize { get; set; }
    }
}

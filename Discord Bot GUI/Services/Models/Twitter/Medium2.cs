using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Twitter
{
    public class Medium2
    {
        [JsonProperty("faces")]
        public List<Face> Faces { get; set; }

        [JsonProperty("h")]
        public int H { get; set; }

        [JsonProperty("w")]
        public int W { get; set; }

        [JsonProperty("resize")]
        public string Resize { get; set; }
    }
}

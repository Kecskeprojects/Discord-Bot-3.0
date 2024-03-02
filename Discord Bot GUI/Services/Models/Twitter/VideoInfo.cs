using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Twitter
{
    public class VideoInfo
    {
        [JsonProperty("aspect_ratio")]
        public List<int> AspectRatio { get; set; }

        [JsonProperty("duration_millis")]
        public int DurationMillis { get; set; }

        [JsonProperty("variants")]
        public List<Variant> Variants { get; set; }
    }
}

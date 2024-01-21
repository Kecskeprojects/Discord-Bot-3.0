using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Twitter
{
    public class OriginalInfo
    {
        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("focus_rects")]
        public List<object> FocusRects { get; set; }
    }
}

using Newtonsoft.Json;

namespace Discord_Bot.Services.Models.Instagram.OtherSubClasses
{
    public class Dimensions
    {
        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }
    }
}

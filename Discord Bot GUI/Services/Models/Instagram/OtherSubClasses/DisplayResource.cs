using Newtonsoft.Json;

namespace Discord_Bot.Services.Models.Instagram.OtherSubClasses;
public class DisplayResource
{
    [JsonProperty("config_height")]
    public int ConfigHeight { get; set; }

    [JsonProperty("config_width")]
    public int ConfigWidth { get; set; }

    [JsonProperty("src")]
    public string Src { get; set; }
}

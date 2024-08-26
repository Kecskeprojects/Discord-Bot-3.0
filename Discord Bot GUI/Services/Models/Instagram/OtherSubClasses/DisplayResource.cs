using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Instagram.OtherSubClasses;

public class DisplayResource
{
    [JsonProperty("config_height")]
    [JsonPropertyName("config_height")]
    public int ConfigHeight { get; set; }

    [JsonProperty("config_width")]
    [JsonPropertyName("config_width")]
    public int ConfigWidth { get; set; }

    [JsonProperty("src")]
    [JsonPropertyName("src")]
    public string Src { get; set; }
}

using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Instagram.OtherSubClasses;

public class DisplayResource
{
    [JsonPropertyName("config_height")]
    public int ConfigHeight { get; set; }

    [JsonPropertyName("config_width")]
    public int ConfigWidth { get; set; }

    [JsonPropertyName("src")]
    public string Src { get; set; }
}

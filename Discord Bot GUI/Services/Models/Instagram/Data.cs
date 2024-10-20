using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Instagram;
public class Data
{
    [JsonProperty("xdt_shortcode_media")]
    [JsonPropertyName("xdt_shortcode_media")]
    public XdtShortcodeMedia XdtShortcodeMedia { get; set; }
}

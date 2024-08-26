using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Instagram.OtherSubClasses;

public class DashInfo
{
    [JsonProperty("is_dash_eligible")]
    [JsonPropertyName("is_dash_eligible")]
    public bool IsDashEligible { get; set; }

    [JsonProperty("number_of_qualities")]
    [JsonPropertyName("number_of_qualities")]
    public int NumberOfQualities { get; set; }

    [JsonProperty("video_dash_manifest")]
    [JsonPropertyName("video_dash_manifest")]
    public object VideoDashManifest { get; set; }
}

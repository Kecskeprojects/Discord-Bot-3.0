using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Instagram.OtherSubClasses;

public class DashInfo
{
    [JsonPropertyName("is_dash_eligible")]
    public bool IsDashEligible { get; set; }

    [JsonPropertyName("number_of_qualities")]
    public int NumberOfQualities { get; set; }

    [JsonPropertyName("video_dash_manifest")]
    public object VideoDashManifest { get; set; }
}

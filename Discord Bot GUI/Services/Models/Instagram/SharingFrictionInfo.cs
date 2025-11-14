using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Instagram;

public class SharingFrictionInfo
{
    [JsonProperty("should_have_sharing_friction")]
    [JsonPropertyName("should_have_sharing_friction")]
    public bool ShouldHaveSharingFriction { get; set; }

    [JsonProperty("bloks_app_url")]
    [JsonPropertyName("bloks_app_url")]
    public object BloksAppUrl { get; set; }
}

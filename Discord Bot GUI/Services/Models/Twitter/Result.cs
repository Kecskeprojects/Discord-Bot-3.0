using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class Result
{
    [JsonProperty("__typename")]
    [JsonPropertyName("__typename")]
    public string Typename { get; set; }

    [JsonProperty("rest_id")]
    [JsonPropertyName("rest_id")]
    public string RestId { get; set; }

    [JsonProperty("core")]
    [JsonPropertyName("core")]
    public Core Core { get; set; }

    [JsonProperty("unmention_data")]
    [JsonPropertyName("unmention_data")]
    public UnmentionData UnmentionData { get; set; }

    [JsonProperty("edit_control")]
    [JsonPropertyName("edit_control")]
    public EditControl EditControl { get; set; }

    [JsonProperty("is_translatable")]
    [JsonPropertyName("is_translatable")]
    public bool IsTranslatable { get; set; }

    [JsonProperty("views")]
    [JsonPropertyName("views")]
    public Views Views { get; set; }

    [JsonProperty("source")]
    [JsonPropertyName("source")]
    public string Source { get; set; }

    [JsonProperty("quoted_status_result")]
    [JsonPropertyName("quoted_status_result")]
    public QuotedStatusResult QuotedStatusResult { get; set; }

    [JsonProperty("legacy")]
    [JsonPropertyName("legacy")]
    public Legacy Legacy { get; set; }

    [JsonProperty("id")]
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonProperty("affiliates_highlighted_label")]
    [JsonPropertyName("affiliates_highlighted_label")]
    public AffiliatesHighlightedLabel AffiliatesHighlightedLabel { get; set; }

    [JsonProperty("is_blue_verified")]
    [JsonPropertyName("is_blue_verified")]
    public bool IsBlueVerified { get; set; }

    [JsonProperty("profile_image_shape")]
    [JsonPropertyName("profile_image_shape")]
    public string ProfileImageShape { get; set; }

    [JsonProperty("professional")]
    [JsonPropertyName("professional")]
    public Professional Professional { get; set; }

    [JsonProperty("reason")]
    [JsonPropertyName("reason")]
    public string Reason { get; set; }

    [JsonProperty("media_key")]
    [JsonPropertyName("media_key")]
    public string MediaKey { get; set; }
}

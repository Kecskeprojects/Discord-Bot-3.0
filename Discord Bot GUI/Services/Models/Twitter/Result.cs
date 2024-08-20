using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class Result
{
    [JsonPropertyName("__typename")]
    public string Typename { get; set; }

    [JsonPropertyName("rest_id")]
    public string RestId { get; set; }

    [JsonPropertyName("core")]
    public Core Core { get; set; }

    [JsonPropertyName("unmention_data")]
    public UnmentionData UnmentionData { get; set; }

    [JsonPropertyName("edit_control")]
    public EditControl EditControl { get; set; }

    [JsonPropertyName("is_translatable")]
    public bool IsTranslatable { get; set; }

    [JsonPropertyName("views")]
    public Views Views { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; }

    [JsonPropertyName("quoted_status_result")]
    public QuotedStatusResult QuotedStatusResult { get; set; }

    [JsonPropertyName("legacy")]
    public Legacy Legacy { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("affiliates_highlighted_label")]
    public AffiliatesHighlightedLabel AffiliatesHighlightedLabel { get; set; }

    [JsonPropertyName("is_blue_verified")]
    public bool IsBlueVerified { get; set; }

    [JsonPropertyName("profile_image_shape")]
    public string ProfileImageShape { get; set; }

    [JsonPropertyName("professional")]
    public Professional Professional { get; set; }

    [JsonPropertyName("reason")]
    public string Reason { get; set; }

    [JsonPropertyName("media_key")]
    public string MediaKey { get; set; }
}

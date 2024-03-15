using Newtonsoft.Json;

namespace Discord_Bot.Services.Models.Twitter
{
    public class Result
    {
        [JsonProperty("__typename")]
        public string Typename { get; set; }

        [JsonProperty("rest_id")]
        public string RestId { get; set; }

        [JsonProperty("core")]
        public Core Core { get; set; }

        [JsonProperty("unmention_data")]
        public UnmentionData UnmentionData { get; set; }

        [JsonProperty("edit_control")]
        public EditControl EditControl { get; set; }

        [JsonProperty("is_translatable")]
        public bool IsTranslatable { get; set; }

        [JsonProperty("views")]
        public Views Views { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("quoted_status_result")]
        public QuotedStatusResult QuotedStatusResult { get; set; }

        [JsonProperty("legacy")]
        public Legacy Legacy { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("affiliates_highlighted_label")]
        public AffiliatesHighlightedLabel AffiliatesHighlightedLabel { get; set; }

        [JsonProperty("is_blue_verified")]
        public bool IsBlueVerified { get; set; }

        [JsonProperty("profile_image_shape")]
        public string ProfileImageShape { get; set; }

        [JsonProperty("professional")]
        public Professional Professional { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("media_key")]
        public string MediaKey { get; set; }
    }
}

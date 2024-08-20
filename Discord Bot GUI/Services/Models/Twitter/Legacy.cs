using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Twitter;

public class Legacy
{
    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; }

    [JsonPropertyName("default_profile")]
    public bool DefaultProfile { get; set; }

    [JsonPropertyName("default_profile_image")]
    public bool DefaultProfileImage { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("entities")]
    public Entities Entities { get; set; }

    [JsonPropertyName("fast_followers_count")]
    public int FastFollowersCount { get; set; }

    [JsonPropertyName("favourites_count")]
    public int FavouritesCount { get; set; }

    [JsonPropertyName("followers_count")]
    public int FollowersCount { get; set; }

    [JsonPropertyName("friends_count")]
    public int FriendsCount { get; set; }

    [JsonPropertyName("has_custom_timelines")]
    public bool HasCustomTimelines { get; set; }

    [JsonPropertyName("is_translator")]
    public bool IsTranslator { get; set; }

    [JsonPropertyName("listed_count")]
    public int ListedCount { get; set; }

    [JsonPropertyName("location")]
    public string Location { get; set; }

    [JsonPropertyName("media_count")]
    public int MediaCount { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("normal_followers_count")]
    public int NormalFollowersCount { get; set; }

    [JsonPropertyName("pinned_tweet_ids_str")]
    public List<string> PinnedTweetIdsStr { get; set; }

    [JsonPropertyName("possibly_sensitive")]
    public bool PossiblySensitive { get; set; }

    [JsonPropertyName("profile_banner_url")]
    public string ProfileBannerUrl { get; set; }

    [JsonPropertyName("profile_image_url_https")]
    public string ProfileImageUrlHttps { get; set; }

    [JsonPropertyName("profile_interstitial_type")]
    public string ProfileInterstitialType { get; set; }

    [JsonPropertyName("screen_name")]
    public string ScreenName { get; set; }

    [JsonPropertyName("statuses_count")]
    public int StatusesCount { get; set; }

    [JsonPropertyName("translator_type")]
    public string TranslatorType { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("verified")]
    public bool Verified { get; set; }

    [JsonPropertyName("withheld_in_countries")]
    public List<object> WithheldInCountries { get; set; }

    [JsonPropertyName("bookmark_count")]
    public int BookmarkCount { get; set; }

    [JsonPropertyName("bookmarked")]
    public bool Bookmarked { get; set; }

    [JsonPropertyName("conversation_id_str")]
    public string ConversationIdStr { get; set; }

    [JsonPropertyName("display_text_range")]
    public List<int> DisplayTextRange { get; set; }

    [JsonPropertyName("extended_entities")]
    public ExtendedEntities ExtendedEntities { get; set; }

    [JsonPropertyName("favorite_count")]
    public int FavoriteCount { get; set; }

    [JsonPropertyName("favorited")]
    public bool Favorited { get; set; }

    [JsonPropertyName("full_text")]
    public string FullText { get; set; }

    [JsonPropertyName("is_quote_status")]
    public bool IsQuoteStatus { get; set; }

    [JsonPropertyName("lang")]
    public string Lang { get; set; }

    [JsonPropertyName("possibly_sensitive_editable")]
    public bool PossiblySensitiveEditable { get; set; }

    [JsonPropertyName("quote_count")]
    public int QuoteCount { get; set; }

    [JsonPropertyName("reply_count")]
    public int ReplyCount { get; set; }

    [JsonPropertyName("retweet_count")]
    public int RetweetCount { get; set; }

    [JsonPropertyName("retweeted")]
    public bool Retweeted { get; set; }

    [JsonPropertyName("user_id_str")]
    public string UserIdStr { get; set; }

    [JsonPropertyName("id_str")]
    public string IdStr { get; set; }

    [JsonPropertyName("quoted_status_id_str")]
    public string QuotedStatusIdStr { get; set; }

    [JsonPropertyName("quoted_status_permalink")]
    public QuotedStatusPermalink QuotedStatusPermalink { get; set; }
}

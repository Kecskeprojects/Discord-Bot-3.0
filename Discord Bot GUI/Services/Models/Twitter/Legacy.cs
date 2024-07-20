using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Twitter;
public class Legacy
{
    [JsonProperty("created_at")]
    public string CreatedAt { get; set; }

    [JsonProperty("default_profile")]
    public bool DefaultProfile { get; set; }

    [JsonProperty("default_profile_image")]
    public bool DefaultProfileImage { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("entities")]
    public Entities Entities { get; set; }

    [JsonProperty("fast_followers_count")]
    public int FastFollowersCount { get; set; }

    [JsonProperty("favourites_count")]
    public int FavouritesCount { get; set; }

    [JsonProperty("followers_count")]
    public int FollowersCount { get; set; }

    [JsonProperty("friends_count")]
    public int FriendsCount { get; set; }

    [JsonProperty("has_custom_timelines")]
    public bool HasCustomTimelines { get; set; }

    [JsonProperty("is_translator")]
    public bool IsTranslator { get; set; }

    [JsonProperty("listed_count")]
    public int ListedCount { get; set; }

    [JsonProperty("location")]
    public string Location { get; set; }

    [JsonProperty("media_count")]
    public int MediaCount { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("normal_followers_count")]
    public int NormalFollowersCount { get; set; }

    [JsonProperty("pinned_tweet_ids_str")]
    public List<string> PinnedTweetIdsStr { get; set; }

    [JsonProperty("possibly_sensitive")]
    public bool PossiblySensitive { get; set; }

    [JsonProperty("profile_banner_url")]
    public string ProfileBannerUrl { get; set; }

    [JsonProperty("profile_image_url_https")]
    public string ProfileImageUrlHttps { get; set; }

    [JsonProperty("profile_interstitial_type")]
    public string ProfileInterstitialType { get; set; }

    [JsonProperty("screen_name")]
    public string ScreenName { get; set; }

    [JsonProperty("statuses_count")]
    public int StatusesCount { get; set; }

    [JsonProperty("translator_type")]
    public string TranslatorType { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("verified")]
    public bool Verified { get; set; }

    [JsonProperty("withheld_in_countries")]
    public List<object> WithheldInCountries { get; set; }

    [JsonProperty("bookmark_count")]
    public int BookmarkCount { get; set; }

    [JsonProperty("bookmarked")]
    public bool Bookmarked { get; set; }

    [JsonProperty("conversation_id_str")]
    public string ConversationIdStr { get; set; }

    [JsonProperty("display_text_range")]
    public List<int> DisplayTextRange { get; set; }

    [JsonProperty("extended_entities")]
    public ExtendedEntities ExtendedEntities { get; set; }

    [JsonProperty("favorite_count")]
    public int FavoriteCount { get; set; }

    [JsonProperty("favorited")]
    public bool Favorited { get; set; }

    [JsonProperty("full_text")]
    public string FullText { get; set; }

    [JsonProperty("is_quote_status")]
    public bool IsQuoteStatus { get; set; }

    [JsonProperty("lang")]
    public string Lang { get; set; }

    [JsonProperty("possibly_sensitive_editable")]
    public bool PossiblySensitiveEditable { get; set; }

    [JsonProperty("quote_count")]
    public int QuoteCount { get; set; }

    [JsonProperty("reply_count")]
    public int ReplyCount { get; set; }

    [JsonProperty("retweet_count")]
    public int RetweetCount { get; set; }

    [JsonProperty("retweeted")]
    public bool Retweeted { get; set; }

    [JsonProperty("user_id_str")]
    public string UserIdStr { get; set; }

    [JsonProperty("id_str")]
    public string IdStr { get; set; }

    [JsonProperty("quoted_status_id_str")]
    public string QuotedStatusIdStr { get; set; }

    [JsonProperty("quoted_status_permalink")]
    public QuotedStatusPermalink QuotedStatusPermalink { get; set; }
}

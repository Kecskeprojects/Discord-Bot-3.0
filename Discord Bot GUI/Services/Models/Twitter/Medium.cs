using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class Medium
{
    [JsonProperty("display_url")]
    [JsonPropertyName("display_url")]
    public string DisplayUrl { get; set; }

    [JsonProperty("expanded_url")]
    [JsonPropertyName("expanded_url")]
    public string ExpandedUrl { get; set; }

    [JsonProperty("id_str")]
    [JsonPropertyName("id_str")]
    public string IdStr { get; set; }

    [JsonProperty("indices")]
    [JsonPropertyName("indices")]
    public List<int> Indices { get; set; }

    [JsonProperty("media_key")]
    [JsonPropertyName("media_key")]
    public string MediaKey { get; set; }

    [JsonProperty("media_url_https")]
    [JsonPropertyName("media_url_https")]
    public string MediaUrlHttps { get; set; }

    [JsonProperty("type")]
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonProperty("url")]
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonProperty("additional_media_info")]
    [JsonPropertyName("additional_media_info")]
    public AdditionalMediaInfo AdditionalMediaInfo { get; set; }

    [JsonProperty("ext_media_availability")]
    [JsonPropertyName("ext_media_availability")]
    public ExtMediaAvailability ExtMediaAvailability { get; set; }

    [JsonProperty("features")]
    [JsonPropertyName("features")]
    public Features Features { get; set; }

    [JsonProperty("sizes")]
    [JsonPropertyName("sizes")]
    public Sizes Sizes { get; set; }

    [JsonProperty("original_info")]
    [JsonPropertyName("original_info")]
    public OriginalInfo OriginalInfo { get; set; }

    [JsonProperty("allow_download_status")]
    [JsonPropertyName("allow_download_status")]
    public AllowDownloadStatus AllowDownloadStatus { get; set; }

    [JsonProperty("video_info")]
    [JsonPropertyName("video_info")]
    public VideoInfo VideoInfo { get; set; }

    [JsonProperty("media_results")]
    [JsonPropertyName("media_results")]
    public MediaResults MediaResults { get; set; }
}

using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Twitter;

public class Medium
{
    [JsonPropertyName("display_url")]
    public string DisplayUrl { get; set; }

    [JsonPropertyName("expanded_url")]
    public string ExpandedUrl { get; set; }

    [JsonPropertyName("id_str")]
    public string IdStr { get; set; }

    [JsonPropertyName("indices")]
    public List<int> Indices { get; set; }

    [JsonPropertyName("media_key")]
    public string MediaKey { get; set; }

    [JsonPropertyName("media_url_https")]
    public string MediaUrlHttps { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("additional_media_info")]
    public AdditionalMediaInfo AdditionalMediaInfo { get; set; }

    [JsonPropertyName("ext_media_availability")]
    public ExtMediaAvailability ExtMediaAvailability { get; set; }

    [JsonPropertyName("features")]
    public Features Features { get; set; }

    [JsonPropertyName("sizes")]
    public Sizes Sizes { get; set; }

    [JsonPropertyName("original_info")]
    public OriginalInfo OriginalInfo { get; set; }

    [JsonPropertyName("allow_download_status")]
    public AllowDownloadStatus AllowDownloadStatus { get; set; }

    [JsonPropertyName("video_info")]
    public VideoInfo VideoInfo { get; set; }

    [JsonPropertyName("media_results")]
    public MediaResults MediaResults { get; set; }
}

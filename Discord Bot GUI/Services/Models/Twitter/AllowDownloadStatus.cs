using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class AllowDownloadStatus
{
    [JsonPropertyName("allow_download")]
    public bool AllowDownload { get; set; }
}

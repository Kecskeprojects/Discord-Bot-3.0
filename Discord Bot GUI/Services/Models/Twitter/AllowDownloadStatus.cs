using Newtonsoft.Json;

namespace Discord_Bot.Services.Models.Twitter;

public class AllowDownloadStatus
{
    [JsonProperty("allow_download")]
    public bool AllowDownload { get; set; }
}

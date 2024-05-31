using System.Diagnostics;

namespace Discord_Bot.Interfaces.Services;

public interface IYoutubeDownloadService
{
    Process CreateYoutubeStream(string url);
}

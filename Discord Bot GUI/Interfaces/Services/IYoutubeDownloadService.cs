using Discord_Bot.Communication;

namespace Discord_Bot.Interfaces.Services;

public interface IYoutubeDownloadService
{
    Task StreamAsync(ServerAudioResource audioResource, string url);
}

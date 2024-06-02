using Discord_Bot.Communication;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Services;

public interface IYoutubeDownloadService
{
    Task StreamAsync(ServerAudioResource audioResource, string url);
}

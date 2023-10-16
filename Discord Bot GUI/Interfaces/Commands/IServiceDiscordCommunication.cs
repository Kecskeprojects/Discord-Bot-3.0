using Discord_Bot.Resources;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Commands
{
    public interface IServiceDiscordCommunication
    {
        Task SendTwitchEmbed(TwitchChannelResource channel, string thumbnailUrl, string title);
        Task<bool> YoutubeAddPlaylistMessage(ulong channelId);
    }
}

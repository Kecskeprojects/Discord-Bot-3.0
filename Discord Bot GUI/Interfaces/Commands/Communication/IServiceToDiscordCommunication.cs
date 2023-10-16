using Discord_Bot.Resources;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Commands.Communication
{
    public interface IServiceToDiscordCommunication
    {
        Task SendTwitchEmbed(TwitchChannelResource channel, string thumbnailUrl, string title);
        Task<bool> YoutubeAddPlaylistMessage(ulong channelId);
    }
}

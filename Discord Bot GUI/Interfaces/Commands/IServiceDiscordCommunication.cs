using Discord_Bot.Resources;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Commands
{
    public interface IServiceDiscordCommunication
    {
        Task SendBirthdayMessage(BirthdayResource birthday);
        Task SendInstagramPostEmbed(Uri uri, ulong messageId, ulong channelId, ulong? guildId);
        Task SendReminder(ReminderResource reminder);
        Task SendTwitchEmbed(TwitchChannelResource channel, string thumbnailUrl, string title);
        Task<bool> YoutubeAddPlaylistMessage(ulong channelId);
    }
}

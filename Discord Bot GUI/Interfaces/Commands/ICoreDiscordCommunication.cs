using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Commands
{
    public interface ICoreDiscordCommunication
    {
        Task SendBirthdayMessages();
        Task SendReminders();
        Task CustomCommands(SocketCommandContext context);
        Task FeatureChecksAsync(SocketCommandContext context);
        Task SendInstagramPostEmbed(Uri uri, ulong messageId, ulong channelId, ulong? guildId);
        Task SelfRole(SocketCommandContext context);
        Task GreetAsync(SocketCommandContext context);
    }
}

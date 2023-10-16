using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Commands.Communication
{
    public interface ICoreToDiscordCommunication
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

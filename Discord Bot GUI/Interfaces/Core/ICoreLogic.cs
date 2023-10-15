using Discord.WebSocket;
using Discord_Bot.Resources;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Core
{
    public interface ICoreLogic
    {
        Task<ServerResource> GetServerAsync(ulong serverId, string serverName);
        Task CustomCommands(ulong serverId, string message, ISocketMessageChannel channel);
        Task SelfRole(ulong serverId, string message, ISocketMessageChannel channel, SocketUser user);
        Task GreetAsync(ISocketMessageChannel channel);
        Task FeatureChecks(ulong serverId, string message, ISocketMessageChannel channel);
        Task ReminderCheck();
        Task BirthdayCheck();
        void Closing();
        void Closing(object sender, EventArgs e);
        void InstagramEmbed(string message, ulong messageId, ulong channelId, ulong? guildId);
        void LogToFile();
        void CheckFolders();
    }
}

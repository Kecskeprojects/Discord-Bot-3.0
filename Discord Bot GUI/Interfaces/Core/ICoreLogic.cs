using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Core
{
    public interface ICoreLogic
    {
        Task CustomCommands(ulong serverId, string message, ISocketMessageChannel channel);
        Task SelfRole(ulong serverId, string message, ISocketMessageChannel channel, SocketUser user);
        Task FeatureChecks(ulong serverId, string message, ISocketMessageChannel channel);
        Task ReminderCheck();
        void Closing();
        void Closing(object sender, EventArgs e);
        void InstagramEmbed(string message, ulong messageId, ulong channelId, ulong? guildId);
        void LogToFile();
        void CheckFolders();
    }
}

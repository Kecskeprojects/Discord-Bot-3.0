using Discord_Bot.Resources;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Core
{
    public interface ICoreLogic
    {
        Task<ServerResource> GetServerAsync(ulong serverId, string serverName);
        void Closing();
        void Closing(object sender, EventArgs e);
        void InstagramEmbed(string message, ulong messageId, ulong channelId, ulong? guildId);
        void LogToFile();
        void CheckFolders();
    }
}

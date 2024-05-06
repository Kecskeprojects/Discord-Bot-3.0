using Discord_Bot.Resources;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Core
{
    public interface ICoreLogic
    {
        Task<ServerResource> GetOrAddServerAsync(ulong serverId, string serverName);
        void InstagramEmbed(string message, ulong messageId, ulong channelId, ulong? guildId);
        void LogToFile();
        void CheckFolders();
    }
}

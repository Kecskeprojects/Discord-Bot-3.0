using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices
{
    public interface IServerService
    {
        Task<DbProcessResultEnum> AddServerAsync(ulong serverId);
        Task<ServerResource> GetByDiscordIdAsync(ulong serverId);
    }
}

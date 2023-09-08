using Discord_Bot.Database.Models;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBRepositories
{
    public interface IServerRepository
    {
        Task AddServerAsync(Server server);
        Task<Server> GetByDiscordIdAsync(ulong serverId);
    }
}

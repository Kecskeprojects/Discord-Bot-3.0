using Discord_Bot.Resources;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices
{
    public interface IServerService
    {
        public Task<ServerResource> GetByDiscordIdAsync(ulong id);
    }
}

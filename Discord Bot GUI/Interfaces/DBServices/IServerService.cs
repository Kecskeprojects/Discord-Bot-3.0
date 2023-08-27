using Discord_Bot.Resources;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices
{
    public interface IServerService
    {
        Task AddServerAsync(ulong id);
        Task AddToMusicRequestAsync(MusicRequest musicRequest, ulong discordId);
        public Task<ServerResource> GetByDiscordIdAsync(ulong id);
    }
}

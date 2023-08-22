using Discord_Bot.Core.Logger;
using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories
{
    public class ServerRepository : BaseRepository, IServerRepository
    {
        public ServerRepository(MainDbContext context, Logging logger) : base(context, logger)
        {
        }

        public Task<Server> GetByDiscordIdAsync(ulong id)
        {
            return context.Servers
                    .Include(x => x.TwitchChannels)
                    .FirstOrDefaultAsync(x => x.DiscordId == id.ToString());
        }
    }
}

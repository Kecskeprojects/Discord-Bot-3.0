using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories
{
    public class ServerRepository(MainDbContext context) : BaseRepository(context), IServerRepository
    {
        public async Task AddServerAsync(Server server)
        {
            await context.Servers.AddAsync(server);
            await context.SaveChangesAsync();
        }

        public Task<Server> GetByDiscordIdAsync(ulong serverId)
        {
            return context.Servers
                    .Include(s => s.TwitchChannels)
                    .Include(s => s.NotificationRole)
                    .FirstOrDefaultAsync(x => x.DiscordId == serverId.ToString());
        }

        public async Task UpdateServerAsync(Server server)
        {
            context.Servers.Update(server);
            await context.SaveChangesAsync();
        }
    }
}

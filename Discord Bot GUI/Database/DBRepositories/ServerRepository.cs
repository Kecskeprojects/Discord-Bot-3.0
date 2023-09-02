using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories
{
    public class ServerRepository : BaseRepository, IServerRepository
    {
        public ServerRepository(MainDbContext context) : base(context)
        {
        }

        public async Task AddServerAsync(Server server)
        {
            await context.Servers.AddAsync(server);
            await context.SaveChangesAsync();
        }

        public Task<Server> GetByDiscordIdAsync(ulong id)
        {
            return context.Servers
                    .FirstOrDefaultAsync(x => x.DiscordId == id.ToString());
        }
    }
}

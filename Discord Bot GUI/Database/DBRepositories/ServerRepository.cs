using Discord_Bot.Core.Logger;
using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories
{
    public class ServerRepository : BaseRepository, IServerRepository
    {
        public ServerRepository(MainDbContext context, Logging logger) : base(context, logger)
        {
        }

        public Task<List<Server>> GetAllServerAsync()
        {
            return context.Servers
                .Include(x => x.TwitchChannels)
                .Include(x => x.Channels).ThenInclude(z => z.ChannelTypes)
                .Include(x => x.CustomCommands)
                .Include(x => x.Keywords)
                .Include(x => x.Roles)
                .ToListAsync();
        }
    }
}

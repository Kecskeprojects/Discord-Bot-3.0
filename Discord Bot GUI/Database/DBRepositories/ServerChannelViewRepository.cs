using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories
{
    public class ServerChannelViewRepository(MainDbContext context) : BaseRepository(context), IServerChannelViewRepository
    {
        public Task<List<ServerChannelView>> GetServerChannels(int serverId)
        {
            return context.ServerChannelViews
                .Where(scv => scv.ServerId == serverId)
                .ToListAsync();
        }
    }
}

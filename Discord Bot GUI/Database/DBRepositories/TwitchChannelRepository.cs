using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories
{
    internal class TwitchChannelRepository : BaseRepository, ITwitchChannelRepository
    {
        public TwitchChannelRepository(MainDbContext context) : base(context)
        {
        }

        public Task<List<TwitchChannel>> GetChannelsAsync()
        {
            return context.TwitchChannels
                .Include(tc => tc.Server)
                .ToListAsync();
        }
    }
}

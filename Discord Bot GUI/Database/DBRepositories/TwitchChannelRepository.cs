using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories
{
    internal class TwitchChannelRepository : BaseRepository, ITwitchChannelRepository
    {
        public TwitchChannelRepository(MainDbContext context) : base(context)
        {
        }

        public async Task AddTwitchChannelAsync(TwitchChannel channel)
        {
            context.TwitchChannels.Add(channel);
            await context.SaveChangesAsync();
        }

        public Task<List<TwitchChannel>> GetChannelsAsync()
        {
            return context.TwitchChannels
                .Include(tc => tc.Server)
                .ThenInclude(tc => tc.Role)
                .ToListAsync();
        }

        public Task<List<TwitchChannel>> GetChannelsByServerIdAsync(ulong serverId)
        {
            return context.TwitchChannels
                .Where(tc => tc.Server.DiscordId == serverId.ToString())
                .ToListAsync();
        }

        public Task<TwitchChannel> GetChannelsNameAsync(ulong serverId, string name)
        {
            return context.TwitchChannels
                .FirstOrDefaultAsync(tc => tc.Server.DiscordId == serverId.ToString() && tc.TwitchName == name);
        }

        public async Task RemoveTwitchChannelAsync(TwitchChannel channel)
        {
            context.TwitchChannels.Remove(channel);
            await context.SaveChangesAsync();
        }

        public async Task RemoveTwitchChannelsAsync(List<TwitchChannel> channels)
        {
            context.TwitchChannels.RemoveRange(channels);
            await context.SaveChangesAsync();
        }

        public Task<bool> TwitchChannelExistsAsync(ulong serverId, string twitchUserId)
        {
            return context.TwitchChannels
                .Where(tc => tc.Server.DiscordId == serverId.ToString() && tc.TwitchId == twitchUserId)
                .AnyAsync();
        }
    }
}

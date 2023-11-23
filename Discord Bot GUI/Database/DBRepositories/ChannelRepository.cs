using Discord_Bot.Database.Models;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories
{
    public class ChannelRepository(MainDbContext context) : BaseRepository(context), IChannelRepository
    {
        public async Task AddChannelAsync(Channel channel)
        {
            context.Channels.Add(channel);
            await context.SaveChangesAsync();
        }

        public Task<Channel> GetChannelByDiscordIdAsync(ulong serverId, ulong channelId)
        {
            return context.Channels
                .Include(c => c.ChannelTypes)
                .FirstOrDefaultAsync(c => c.Server.DiscordId == serverId.ToString() && c.DiscordId == channelId.ToString());
        }

        public Task<List<Channel>> GetChannelsByTypeAsync(ulong serverId, ChannelTypeEnum channelTypeId)
        {
            return context.Channels
                .Include(c => c.ChannelTypes)
                .Where(c => c.Server.DiscordId == serverId.ToString() && c.ChannelTypes.FirstOrDefault(ct => ct.ChannelTypeId == (int)channelTypeId) != null)
                .ToListAsync();
        }

        public Task<List<Channel>> GetChannelsByTypeExcludingCurrentAsync(ulong serverId, ChannelTypeEnum channelTypeId, ulong channelId)
        {
            return context.Channels
                .Include(c => c.ChannelTypes)
                .Where(c => c.Server.DiscordId == serverId.ToString() && c.DiscordId != channelId.ToString() && c.ChannelTypes.FirstOrDefault(ct => ct.ChannelTypeId == (int)channelTypeId) != null)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}

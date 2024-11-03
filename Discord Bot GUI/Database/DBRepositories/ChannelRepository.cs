using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Resources;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories;

public class ChannelRepository(MainDbContext context) : GenericRepository<Channel>(context), IChannelRepository
{
    public Task<List<ServerChannelResource>> GetServerChannelsAsync(int serverId)
    {
        IQueryable<ServerChannelResource> query =
            from channelType in context.ChannelTypes
            from channels in context.Channels
            where channels.ServerId == serverId && channels.ChannelTypes.Any(x => x.ChannelTypeId == channelType.ChannelTypeId)
            select new ServerChannelResource()
            {
                ChannelTypeId = channelType.ChannelTypeId,
                ChannelDiscordId = channels.DiscordId
            };

        return query.ToListAsync();
    }
}

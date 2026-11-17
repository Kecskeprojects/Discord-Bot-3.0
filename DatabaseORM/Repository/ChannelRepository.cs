using DatabaseORM.Context;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Model;
using DatabaseORM.Resources;
using Microsoft.EntityFrameworkCore;

namespace DatabaseORM.Repository;

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

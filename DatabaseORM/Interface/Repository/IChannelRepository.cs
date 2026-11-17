using DatabaseORM.Model;
using DatabaseORM.Resources;

namespace DatabaseORM.Interface.Repository;

public interface IChannelRepository : IGenericRepository<Channel>
{
    Task<List<ServerChannelResource>> GetServerChannelsAsync(int serverId);
}

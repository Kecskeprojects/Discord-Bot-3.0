using Discord_Bot.Database.Models;
using Discord_Bot.Resources;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBRepositories;

public interface IChannelRepository : IGenericRepository<Channel>
{
    Task<List<ServerChannelResource>> GetServerChannelsAsync(int serverId);
}

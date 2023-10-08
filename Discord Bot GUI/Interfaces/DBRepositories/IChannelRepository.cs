using Discord_Bot.Database.Models;
using Discord_Bot.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBRepositories
{
    public interface IChannelRepository
    {
        Task AddChannelAsync(Channel channel);
        Task<Channel> GetChannelByDiscordIdAsync(ulong serverId, ulong channelId);
        Task<List<Channel>> GetChannelsByTypeAsync(ulong serverId, ChannelTypeEnum channelTypeId);
        Task<List<Channel>> GetChannelsByTypeExcludingCurrentAsync(ulong serverId, ChannelTypeEnum channelTypeId, ulong channelId);
        Task SaveChangesAsync();
    }
}

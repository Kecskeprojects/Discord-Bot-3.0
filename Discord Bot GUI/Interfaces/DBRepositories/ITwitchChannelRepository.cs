using Discord_Bot.Database.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBRepositories
{
    public interface ITwitchChannelRepository
    {
        Task AddTwitchChannelAsync(TwitchChannel channel);
        Task<List<TwitchChannel>> GetChannelsAsync();
        Task<List<TwitchChannel>> GetChannelsByServerIdAsync(ulong serverId);
        Task<TwitchChannel> GetChannelsNameAsync(ulong serverId, string name);
        Task RemoveTwitchChannelAsync(TwitchChannel channel);
        Task RemoveTwitchChannelsAsync(List<TwitchChannel> channels);
        Task<bool> TwitchChannelExistsAsync(ulong serverId, string twitchUserId);
    }
}

using DatabaseORM.Enums;
using DatabaseORM.Resources;

namespace DatabaseORM.Interface.Service;

public interface ITwitchChannelService
{
    Task<DbProcessResultEnum> AddTwitchChannelAsync(ulong serverId, string twitchUserId, string twitchUserName, string twitchLink);
    Task<List<TwitchChannelResource>> GetChannelsAsync();
    Task<DbProcessResultEnum> RemoveTwitchChannelAsync(ulong serverId, string name = "");
}

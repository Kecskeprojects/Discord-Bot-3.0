using Discord_Bot.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices;

public interface IChannelService
{
    Task<DbProcessResultEnum> AddSettingChannelAsync(ulong serverId, ChannelTypeEnum channelTypeId, ulong channelId);
    Task<Dictionary<ChannelTypeEnum, List<ulong>>> GetServerChannelsAsync(int serverId);
    Task<DbProcessResultEnum> RemovelSettingChannelAsync(ulong serverId, ChannelTypeEnum channelTypeId, ulong channelId);
    Task<DbProcessResultEnum> RemoveSettingChannelsAsync(ulong serverId, ChannelTypeEnum channelTypeId);
}

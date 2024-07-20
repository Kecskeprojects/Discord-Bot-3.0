using Discord_Bot.Enums;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices;
public interface IChannelService
{
    Task<DbProcessResultEnum> AddSettingChannelAsync(ulong serverId, ChannelTypeEnum channelTypeId, ulong channelId);
    Task<DbProcessResultEnum> RemovelSettingChannelAsync(ulong serverId, ChannelTypeEnum channelTypeId, ulong channelId);
    Task<DbProcessResultEnum> RemoveSettingChannelsAsync(ulong serverId, ChannelTypeEnum channelTypeId);
}

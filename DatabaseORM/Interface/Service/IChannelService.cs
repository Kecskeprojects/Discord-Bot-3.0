using DatabaseORM.Enums;

namespace DatabaseORM.Interface.Service;

public interface IChannelService
{
    Task<DbProcessResultEnum> AddSettingChannelAsync(ulong serverId, ChannelTypeEnum channelTypeId, ulong channelId);
    Task<Dictionary<ChannelTypeEnum, List<ulong>>> GetServerChannelsAsync(int serverId);
    Task<DbProcessResultEnum> RemoveSettingChannelAsync(ulong serverId, ChannelTypeEnum channelTypeId, ulong channelId);
    Task<DbProcessResultEnum> RemoveSettingChannelsAsync(ulong serverId, ChannelTypeEnum channelTypeId);
}

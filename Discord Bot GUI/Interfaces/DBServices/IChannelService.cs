using Discord_Bot.Enums;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices
{
    public interface IChannelService
    {
        Task<DbProcessResultEnum> AddSettingChannelAsync(ulong serverId, ChannelTypeEnum channelType, ulong channelId);
        Task<DbProcessResultEnum> RemovelSettingChanneAsync(ulong serverId, ChannelTypeEnum channelType, ulong channelId);
        Task<DbProcessResultEnum> RemoveSettingChannelsAsync(ulong id, ChannelTypeEnum channelType);
    }
}

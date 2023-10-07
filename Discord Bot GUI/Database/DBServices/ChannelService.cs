using AutoMapper;
using Discord_Bot.Core.Caching;
using Discord_Bot.Core.Logger;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices
{
    public class ChannelService : BaseService, IChannelService
    {
        public ChannelService(IMapper mapper, Logging logger, Cache cache) : base(mapper, logger, cache)
        {
        }

        public Task<DbProcessResultEnum> AddSettingChannelAsync(ulong serverId, ChannelTypeEnum channelType, ulong channelId) => throw new System.NotImplementedException();
        public Task<DbProcessResultEnum> RemovelSettingChanneAsync(ulong serverId, ChannelTypeEnum channelType, ulong channelId) => throw new System.NotImplementedException();
        public Task<DbProcessResultEnum> RemoveSettingChannelsAsync(ulong id, ChannelTypeEnum channelType) => throw new System.NotImplementedException();
    }
}

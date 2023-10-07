using AutoMapper;
using Discord_Bot.Core.Caching;
using Discord_Bot.Core.Logger;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices
{
    public class ChannelService : BaseService, IChannelService
    {
        private readonly IChannelRepository channelRepository;

        public ChannelService(IChannelRepository channelRepository, IMapper mapper, Logging logger, Cache cache) : base(mapper, logger, cache)
        {
            this.channelRepository = channelRepository;
        }

        public Task<DbProcessResultEnum> AddSettingChannelAsync(ulong serverId, ChannelTypeEnum channelType, ulong channelId) => throw new System.NotImplementedException();
        public Task<DbProcessResultEnum> RemovelSettingChanneAsync(ulong serverId, ChannelTypeEnum channelType, ulong channelId) => throw new System.NotImplementedException();
        public Task<DbProcessResultEnum> RemoveSettingChannelsAsync(ulong id, ChannelTypeEnum channelType) => throw new System.NotImplementedException();
    }
}

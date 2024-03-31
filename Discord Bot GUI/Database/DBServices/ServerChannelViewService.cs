using AutoMapper;
using Discord_Bot.Core;
using Discord_Bot.Core.Caching;
using Discord_Bot.Database.Models;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices
{
    public class ServerChannelViewService(IServerChannelViewRepository serverChannelViewRepository, IMapper mapper, Logging logger, Cache cache) : BaseService(mapper, logger, cache), IServerChannelViewService
    {
        private readonly IServerChannelViewRepository serverChannelViewRepository = serverChannelViewRepository;

        public async Task<Dictionary<ChannelTypeEnum, List<ulong>>> GetServerChannelsAsync(int serverId)
        {
            List<ServerChannelView> channels = await serverChannelViewRepository.GetListAsync(scv => scv.ServerDiscordId == serverId.ToString());

            List<IGrouping<int?, ServerChannelView>> groups = channels.GroupBy(ch => ch.ChannelTypeId).ToList();
            return mapper.Map<List<IGrouping<int?, ServerChannelView>>, Dictionary<ChannelTypeEnum, List<ulong>>>(groups);
        }
    }
}

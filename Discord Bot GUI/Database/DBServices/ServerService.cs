using AutoMapper;
using Discord_Bot.Core;
using Discord_Bot.Core.Logger;
using Discord_Bot.Database.Models;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices
{
    public class ServerService : BaseService, IServerService
    {
        private readonly IServerRepository serverRepository;
        private readonly IServerChannelViewRepository channelViewRepository;

        public ServerService(IServerRepository serverRepository, IServerChannelViewRepository channelViewRepository, IMapper mapper, Logging logger) : base(mapper, logger)
        {
            this.serverRepository = serverRepository;
            this.channelViewRepository = channelViewRepository;
        }

        public async Task<ServerResource> GetByDiscordIdAsync(ulong id)
        {
            try
            {
                if (Global.Cache.ServerCache.ContainsKey(id))
                {
                    return Global.Cache.ServerCache[id];
                }

                Server result = await serverRepository.GetByDiscordIdAsync(id);
                ServerResource resource = mapper.Map<Server, ServerResource>(result);
                await FillWithChannels(resource);

                Global.Cache.ServerCache.Add(resource.DiscordId, resource);

                return resource;
            }
            catch(Exception ex)
            {
                logger.Error("ServerService.cs GetByDiscordIdAsync", ex.ToString());
                return null;
            }
        }

        private async Task FillWithChannels(ServerResource resource)
        {
            List<ServerChannelView> channels = await channelViewRepository.GetServerChannels(resource.ServerId);
            List<IGrouping<int?, ServerChannelView>> groups = channels.GroupBy(ch => ch.ChannelTypeId).ToList();
            resource.SettingsChannels = mapper.Map<List<IGrouping<int?, ServerChannelView>>, Dictionary<ChannelTypeEnum, List<ulong>>>(groups);
            return;
        }
    }
}

using AutoMapper;
using Discord_Bot.Core.Caching;
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

        public ServerService(IServerRepository serverRepository, IServerChannelViewRepository channelViewRepository, IMapper mapper, Logging logger, Cache cache) : base(mapper, logger, cache)
        {
            this.serverRepository = serverRepository;
            this.channelViewRepository = channelViewRepository;
        }

        public async Task AddServerAsync(ulong id)
        {
            try
            {
                Server server = new()
                {
                    DiscordId = id.ToString()
                };
                await serverRepository.AddServerAsync(server);

                logger.Log($"Server added with the following ID: {id}");
            }
            catch (Exception ex)
            {
                logger.Error("ServerService.cs AddServerAsync", ex.ToString());
            }
        }

        public async Task<ServerResource> GetByDiscordIdAsync(ulong id)
        {
            ServerResource result = null;
            try
            {
                ServerResource cachedServer = cache.TryGetValue(id);
                if (cachedServer != null)
                {
                    return cachedServer;
                }

                Server server = await serverRepository.GetByDiscordIdAsync(id);
                if (server == null) return null;

                result = mapper.Map<Server, ServerResource>(server);
                await FillWithChannels(result);

                cache.TryAddValue(result.DiscordId, result);
            }
            catch (Exception ex)
            {
                logger.Error("ServerService.cs GetByDiscordIdAsync", ex.ToString());
            }

            return result;
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

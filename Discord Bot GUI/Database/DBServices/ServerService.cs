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
    public class ServerService(IServerRepository serverRepository, IServerChannelViewRepository channelViewRepository, IMapper mapper, Logging logger, Cache cache) : BaseService(mapper, logger, cache), IServerService
    {
        private readonly IServerRepository serverRepository = serverRepository;
        private readonly IServerChannelViewRepository channelViewRepository = channelViewRepository;

        public async Task<DbProcessResultEnum> AddServerAsync(ulong serverId)
        {
            try
            {
                Server server = new()
                {
                    DiscordId = serverId.ToString()
                };
                await serverRepository.AddServerAsync(server);

                logger.Log($"Server added with the following ID: {serverId}");
                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("ServerService.cs AddServerAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<ServerResource> GetByDiscordIdAsync(ulong serverId)
        {
            ServerResource result = null;
            try
            {
                ServerResource cachedServer = cache.TryGetValue(serverId);
                if (cachedServer != null)
                {
                    return cachedServer;
                }

                Server server = await serverRepository.GetByDiscordIdAsync(serverId);
                if (server == null)
                {
                    return null;
                }

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

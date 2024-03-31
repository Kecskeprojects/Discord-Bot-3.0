using AutoMapper;
using Discord_Bot.Core;
using Discord_Bot.Core.Caching;
using Discord_Bot.Database.Models;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using Discord_Bot.Tools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices
{
    public class TwitchChannelService(IMapper mapper, Logging logger, Cache cache, ITwitchChannelRepository twitchChannelRepository, IServerRepository serverRepository) : BaseService(mapper, logger, cache), ITwitchChannelService
    {
        private readonly ITwitchChannelRepository twitchChannelRepository = twitchChannelRepository;
        private readonly IServerRepository serverRepository = serverRepository;

        public async Task<DbProcessResultEnum> AddTwitchChannelAsync(ulong serverId, string twitchUserId, string twitchUserName)
        {
            try
            {
                string url = $"https://www.twitch.tv/{twitchUserName}";
                Server server = await serverRepository.FirstOrDefaultAsync(s => s.DiscordId == serverId.ToString());
                if (server == null)
                {
                    return DbProcessResultEnum.NotFound;
                }

                if (await twitchChannelRepository.TwitchChannelExistsAsync(serverId, twitchUserId))
                {
                    return DbProcessResultEnum.AlreadyExists;
                }

                TwitchChannel channel = new()
                {
                    TwitchChannelId = 0,
                    TwitchId = twitchUserId,
                    TwitchLink = url,
                    TwitchName = twitchUserName,
                    Server = server
                };

                await twitchChannelRepository.AddTwitchChannelAsync(channel);

                cache.RemoveCachedEntityManually(serverId);

                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("TwitchChannelService.cs AddTwitchChannelAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<List<TwitchChannelResource>> GetChannelsAsync()
        {
            List<TwitchChannelResource> result = null;
            try
            {
                List<TwitchChannel> channels = await twitchChannelRepository.GetChannelsAsync();
                if (channels == null)
                {
                    return null;
                }

                result = mapper.Map<List<TwitchChannel>, List<TwitchChannelResource>>(channels);
            }
            catch (Exception ex)
            {
                logger.Error("TwitchChannelService.cs GetChannelsAsync", ex.ToString());
            }

            return result;
        }

        public async Task<DbProcessResultEnum> RemoveTwitchChannelAsync(ulong serverId, string name)
        {
            try
            {
                TwitchChannel channel = await twitchChannelRepository.GetChannelsNameAsync(serverId, name);
                if (channel == null)
                {
                    return DbProcessResultEnum.NotFound;
                }

                await twitchChannelRepository.RemoveTwitchChannelAsync(channel);

                cache.RemoveCachedEntityManually(serverId);

                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("TwitchChannelService.cs RemoveTwitchChannelAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<DbProcessResultEnum> RemoveTwitchChannelsAsync(ulong serverId)
        {
            try
            {
                List<TwitchChannel> channels = await twitchChannelRepository.GetChannelsByServerIdAsync(serverId);
                if (CollectionTools.IsNullOrEmpty(channels))
                {
                    return DbProcessResultEnum.NotFound;
                }

                await twitchChannelRepository.RemoveTwitchChannelsAsync(channels);

                cache.RemoveCachedEntityManually(serverId);

                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("TwitchChannelService.cs RemoveTwitchChannelsAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }
    }
}

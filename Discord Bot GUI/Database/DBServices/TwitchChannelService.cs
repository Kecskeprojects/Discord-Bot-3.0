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

namespace Discord_Bot.Database.DBServices;

public class TwitchChannelService(IMapper mapper, BotLogger logger, Cache cache, ITwitchChannelRepository twitchChannelRepository, IServerRepository serverRepository) : BaseService(mapper, logger, cache), ITwitchChannelService
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

            if (await twitchChannelRepository.ExistsAsync(
                tc => tc.Server.DiscordId == serverId.ToString()
                && tc.TwitchId == twitchUserId,
                tc => tc.Server))
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

            await twitchChannelRepository.AddAsync(channel);

            cache.RemoveCachedEntityManually(serverId);

            return DbProcessResultEnum.Success;
        }
        catch (Exception ex)
        {
            logger.Error("TwitchChannelService.cs AddTwitchChannelAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }

    public async Task<List<TwitchChannelResource>> GetChannelsAsync()
    {
        List<TwitchChannelResource> result = null;
        try
        {
            List<TwitchChannel> channels = await twitchChannelRepository.GetAllAsync(tc => tc.Server, tc => tc.Server.NotificationRole);
            if (channels == null)
            {
                return null;
            }

            result = mapper.Map<List<TwitchChannel>, List<TwitchChannelResource>>(channels);
        }
        catch (Exception ex)
        {
            logger.Error("TwitchChannelService.cs GetChannelsAsync", ex);
        }

        return result;
    }

    public async Task<DbProcessResultEnum> RemoveTwitchChannelAsync(ulong serverId, string name = "")
    {
        try
        {
            List<TwitchChannel> channels = await twitchChannelRepository.GetListAsync(
                tc => tc.Server.DiscordId == serverId.ToString()
                && (string.IsNullOrEmpty(name) || tc.TwitchName == name),
                tc => tc.Server);
            if (CollectionTools.IsNullOrEmpty(channels))
            {
                return DbProcessResultEnum.NotFound;
            }

            await twitchChannelRepository.RemoveAsync(channels);

            cache.RemoveCachedEntityManually(serverId);

            return DbProcessResultEnum.Success;
        }
        catch (Exception ex)
        {
            logger.Error("TwitchChannelService.cs RemoveTwitchChannelAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }
}

using AutoMapper;
using Discord_Bot.Core.Caching;
using Discord_Bot.Core.Logger;
using Discord_Bot.Database.Models;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices
{
    public class ChannelService : BaseService, IChannelService
    {
        private readonly IChannelRepository channelRepository;
        private readonly IServerRepository serverRepository;
        private readonly IChannelTypeRepository channelTypeRepository;

        public ChannelService(IChannelRepository channelRepository, IServerRepository serverRepository, IChannelTypeRepository channelTypeRepository, IMapper mapper, Logging logger, Cache cache) : base(mapper, logger, cache)
        {
            this.channelRepository = channelRepository;
            this.serverRepository = serverRepository;
            this.channelTypeRepository = channelTypeRepository;
        }

        public async Task<DbProcessResultEnum> AddSettingChannelAsync(ulong serverId, ChannelTypeEnum channelTypeId, ulong channelId)
        {
            try
            {
                Channel channel = await channelRepository.GetChannelByDiscordIdAsync(serverId, channelId);

                ChannelType channelType = await channelTypeRepository.GetChannelTypeByIdAsync(channelTypeId);
                if(channelType == null)
                {
                    return DbProcessResultEnum.NotFound;
                }

                Server server = await serverRepository.GetByDiscordIdAsync(serverId);
                if (channelType == null)
                {
                    return DbProcessResultEnum.NotFound;
                }

                if (ChannelTypeNameCollections.RestrictedChannelTypes.Contains(channelTypeId))
                {
                    List<Channel> channels = await channelRepository.GetChannelsByTypeExcludingCurrentAsync(serverId, channelTypeId, channelId);
                    channels.ForEach((channel) => { channel.ChannelTypes.Remove(channelType); });
                }

                if (channel == null)
                {
                    channel = new()
                    {
                        ChannelId = 0,
                        Server = server,
                        ChannelTypes = new List<ChannelType>() { channelType },
                        DiscordId = channelId.ToString()
                    };
                    await channelRepository.AddChannelAsync(channel);
                }
                else
                {
                    if(channel.ChannelTypes.Contains(channelType))
                    {
                        return DbProcessResultEnum.AlreadyExists;
                    }

                    channel.ChannelTypes.Add(channelType);
                    await channelRepository.SaveChangesAsync();
                }

                cache.RemoveCachedEntityManually(serverId);

                logger.Log("Settings Channel added successfully!");
                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("ChannelService.cs AddSettingChannelAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<DbProcessResultEnum> RemovelSettingChannelAsync(ulong serverId, ChannelTypeEnum channelTypeId, ulong channelId)
        {
            try
            {
                Channel channel = await channelRepository.GetChannelByDiscordIdAsync(serverId, channelId);
                ChannelType channelType = await channelTypeRepository.GetChannelTypeByIdAsync(channelTypeId);
                if (channel != null)
                {
                    if (!channel.ChannelTypes.Remove(channelType))
                    {
                        logger.Log($"No channel of type '{ChannelTypeNameCollections.EnumName[channelTypeId]}' were found!");
                        return DbProcessResultEnum.NotFound;
                    }

                    await channelRepository.SaveChangesAsync();

                    cache.RemoveCachedEntityManually(serverId);
                    logger.Log($"Settings channel of type '{ChannelTypeNameCollections.EnumName[channelTypeId]}' removed successfully!");
                    return DbProcessResultEnum.Success;
                }
                else
                {
                    logger.Log($"No channel of type '{ChannelTypeNameCollections.EnumName[channelTypeId]}' were found!");
                    return DbProcessResultEnum.NotFound;
                }
            }
            catch (Exception ex)
            {
                logger.Error("ChannelService.cs RemoveSettingChannelAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<DbProcessResultEnum> RemoveSettingChannelsAsync(ulong serverId, ChannelTypeEnum channelTypeId)
        {
            try
            {
                List<Channel> channels = await channelRepository.GetChannelsByTypeAsync(serverId, channelTypeId);
                ChannelType channelType = await channelTypeRepository.GetChannelTypeByIdAsync(channelTypeId);
                if (!CollectionTools.IsNullOrEmpty(channels))
                {
                    channels.ForEach((channel) => { channel.ChannelTypes.Remove(channelType); });

                    await channelRepository.SaveChangesAsync();

                    cache.RemoveCachedEntityManually(serverId);
                    logger.Log($"All settings channels of type '{ChannelTypeNameCollections.EnumName[channelTypeId]}' removed successfully!");
                    return DbProcessResultEnum.Success;
                }
                else
                {
                    logger.Log($"No channels of type '{ChannelTypeNameCollections.EnumName[channelTypeId]}' were found!");
                    return DbProcessResultEnum.NotFound;
                }
            }
            catch (Exception ex)
            {
                logger.Error("ChannelService.cs RemoveSettingChannelsAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }
    }
}

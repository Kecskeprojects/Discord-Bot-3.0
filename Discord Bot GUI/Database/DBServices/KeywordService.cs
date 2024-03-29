﻿using AutoMapper;
using Discord_Bot.Core;
using Discord_Bot.Core.Caching;
using Discord_Bot.Database.Models;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices
{
    public class KeywordService(IMapper mapper, Logging logger, Cache cache, IKeywordRepository keywordRepository, IServerRepository serverRepository) : BaseService(mapper, logger, cache), IKeywordService
    {
        private readonly IKeywordRepository keywordRepository = keywordRepository;
        private readonly IServerRepository serverRepository = serverRepository;

        public async Task<DbProcessResultEnum> AddKeywordAsync(ulong serverId, string trigger, string response)
        {
            try
            {
                if (await keywordRepository.KeywordExistsAsync(serverId, trigger))
                {
                    return DbProcessResultEnum.AlreadyExists;
                }

                Server server = await serverRepository.GetByDiscordIdAsync(serverId);

                Keyword keyword = new()
                {
                    KeywordId = 0,
                    Server = server,
                    Trigger = trigger,
                    Response = response
                };
                await keywordRepository.AddCustomCommandAsync(keyword);

                logger.Log("Keyword added successfully!");
                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("KeywordService.cs AddKeywordAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<KeywordResource> GetKeywordAsync(ulong serverId, string trigger)
        {
            KeywordResource result = null;
            try
            {
                Keyword keyword = await keywordRepository.GetKeywordAsync(serverId, trigger);
                result = mapper.Map<Keyword, KeywordResource>(keyword);
            }
            catch (Exception ex)
            {
                logger.Error("KeywordService.cs GetKeywordAsync", ex.ToString());
            }
            return result;
        }

        public async Task<DbProcessResultEnum> RemoveKeywordAsync(ulong serverId, string trigger)
        {
            try
            {
                Keyword keyword = await keywordRepository.GetKeywordAsync(serverId, trigger);
                if (keyword != null)
                {
                    await keywordRepository.RemoveKeywordAsync(keyword);

                    logger.Log($"Keyword '{trigger}' removed successfully!");
                    return DbProcessResultEnum.Success;
                }
                else
                {
                    logger.Log($"Keyword '{trigger}' could not be found!");
                    return DbProcessResultEnum.NotFound;
                }
            }
            catch (Exception ex)
            {
                logger.Error("KeywordService.cs RemoveKeywordAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }
    }
}

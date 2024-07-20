using AutoMapper;
using Discord_Bot.Core;
using Discord_Bot.Core.Caching;
using Discord_Bot.Database.Models;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices;

public class KeywordService(
    IKeywordRepository keywordRepository,
    IServerRepository serverRepository,
    IMapper mapper,
    BotLogger logger,
    ServerCache cache) : BaseService(mapper, logger, cache), IKeywordService
{
    private readonly IKeywordRepository keywordRepository = keywordRepository;
    private readonly IServerRepository serverRepository = serverRepository;

    public async Task<DbProcessResultEnum> AddKeywordAsync(ulong serverId, string trigger, string response)
    {
        try
        {
            trigger = trigger.Trim().ToLower();
            if (await keywordRepository.ExistsAsync(
                kw => kw.Server.DiscordId == serverId.ToString()
                && kw.Trigger.Trim().ToLower().Equals(trigger),
                kw => kw.Server))
            {
                return DbProcessResultEnum.AlreadyExists;
            }

            Server server = await serverRepository.FirstOrDefaultAsync(s => s.DiscordId == serverId.ToString());

            Keyword keyword = new()
            {
                KeywordId = 0,
                Server = server,
                Trigger = trigger,
                Response = response
            };
            await keywordRepository.AddAsync(keyword);

            logger.Log("Keyword added successfully!");
            return DbProcessResultEnum.Success;
        }
        catch (Exception ex)
        {
            logger.Error("KeywordService.cs AddKeywordAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }

    public async Task<KeywordResource> GetKeywordAsync(ulong serverId, string trigger)
    {
        KeywordResource result = null;
        try
        {
            trigger = trigger.Trim().ToLower();
            Keyword keyword = await keywordRepository.FirstOrDefaultAsync(
                kw => kw.Server.DiscordId == serverId.ToString()
                && kw.Trigger.Trim().ToLower().Equals(trigger),
                kw => kw.Server);
            result = mapper.Map<Keyword, KeywordResource>(keyword);
        }
        catch (Exception ex)
        {
            logger.Error("KeywordService.cs GetKeywordAsync", ex);
        }
        return result;
    }

    public async Task<DbProcessResultEnum> RemoveKeywordAsync(ulong serverId, string trigger)
    {
        try
        {
            trigger = trigger.Trim().ToLower();
            Keyword keyword = await keywordRepository.FirstOrDefaultAsync(
                kw => kw.Server.DiscordId == serverId.ToString()
                && kw.Trigger.Trim().ToLower().Equals(trigger),
                kw => kw.Server);
            if (keyword != null)
            {
                await keywordRepository.RemoveAsync(keyword);

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
            logger.Error("KeywordService.cs RemoveKeywordAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }
}

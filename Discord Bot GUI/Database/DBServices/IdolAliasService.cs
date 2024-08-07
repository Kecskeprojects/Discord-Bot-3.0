﻿using AutoMapper;
using Discord_Bot.Core;
using Discord_Bot.Core.Caching;
using Discord_Bot.Database.Models;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices;

public class IdolAliasService(
    IIdolAliasRepository idolAliasRepository,
    IIdolRepository idolRepository,
    IMapper mapper,
    BotLogger logger,
    ServerCache cache) : BaseService(mapper, logger, cache), IIdolAliasService
{
    private readonly IIdolAliasRepository idolAliasRepository = idolAliasRepository;
    private readonly IIdolRepository idolRepository = idolRepository;

    public async Task<DbProcessResultEnum> AddIdolAliasAsync(string idolAlias, string idolName, string idolGroup)
    {
        try
        {
            if (await idolAliasRepository.ExistsAsync(
                ia => ia.Alias == idolAlias
                && ia.Idol.Name == idolName
                && ia.Idol.Group.Name == idolGroup,
                ia => ia.Idol,
                ia => ia.Idol.Group))
            {
                logger.Log($"Idol Alias [{idolAlias}-{idolName}]-[{idolGroup}] is already in database!");
                return DbProcessResultEnum.AlreadyExists;
            }

            Idol idol = await idolRepository.FirstOrDefaultAsync(
                i => idolGroup == i.Group.Name
                && i.Name == idolName,
                i => i.IdolAliases,
                i => i.Group);

            if (idol == null)
            {
                return DbProcessResultEnum.NotFound;
            }

            IdolAlias alias = new()
            {
                IdolId = 0,
                Alias = idolAlias,
            };
            idol.IdolAliases.Add(alias);

            await idolRepository.UpdateAsync(idol);

            logger.Log($"Idol Alias [{idolAlias}-{idolName}]-[{idolGroup}] added successfully!");
            return DbProcessResultEnum.Success;
        }
        catch (Exception ex)
        {
            logger.Error("IdolAliasService.cs AddIdolAliasAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }

    public async Task<DbProcessResultEnum> RemoveIdolAliasAsync(string idolAlias, string idolName, string idolGroup)
    {
        try
        {
            IdolAlias idolAliasItem = await idolAliasRepository.FirstOrDefaultAsync(
                ia => ia.Alias == idolAlias
                && ia.Idol.Name == idolName
                && ia.Idol.Group.Name == idolGroup,
                ia => ia.Idol,
                ia => ia.Idol.Group);
            if (idolAlias != null)
            {
                await idolAliasRepository.RemoveAsync(idolAliasItem);

                logger.Log($"Idol Alias [{idolAlias}-{idolName}]-[{idolGroup}] removed successfully!");
                return DbProcessResultEnum.Success;
            }
            else
            {
                logger.Log($"Idol Alias [{idolAlias}-{idolName}]-[{idolGroup}] could not be found!");
                return DbProcessResultEnum.NotFound;
            }
        }
        catch (Exception ex)
        {
            logger.Error("IdolAliasService.cs RemoveIdolAliasAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }
}

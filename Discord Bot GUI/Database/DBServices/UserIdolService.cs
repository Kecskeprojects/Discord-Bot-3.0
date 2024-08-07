﻿using AutoMapper;
using Discord_Bot.Core;
using Discord_Bot.Core.Caching;
using Discord_Bot.Database.Models;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices;

public class UserIdolService(
    IIdolRepository idolRepository,
    IUserRepository userRepository,
    IMapper mapper,
    BotLogger logger,
    ServerCache cache) : BaseService(mapper, logger, cache), IUserIdolService
{
    private readonly IIdolRepository idolRepository = idolRepository;
    private readonly IUserRepository userRepository = userRepository;

    public async Task<List<IdolResource>> GetUserIdolsListAsync(ulong userId, string groupName)
    {
        List<IdolResource> result = null;
        try
        {
            List<Idol> idols = await idolRepository.GetListAsync(
                i => (string.IsNullOrEmpty(groupName)
                || i.Group.Name == groupName)
                && i.Users.FirstOrDefault(u => u.DiscordId == userId.ToString()) != null,
                orderBy: i => i.Name,
                ascending: true,
                i => i.Users,
                i => i.Group);

            result = mapper.Map<List<Idol>, List<IdolResource>>(idols);
        }
        catch (Exception ex)
        {
            logger.Error("UserIdolService.cs GetUserIdolsListAsync", ex);
        }
        return result;
    }

    public async Task<DbProcessResultEnum> AddUserIdolAsync(ulong userId, string idolName, string idolGroup)
    {
        try
        {
            bool noGroup = string.IsNullOrEmpty(idolGroup);

            List<Idol> idols = await idolRepository.GetListByNamesAsync(idolName, idolGroup);

            if (idols.Count == 0)
            {
                logger.Log($"Idol [{idolName}]-[{(noGroup ? "No group specified" : idolGroup)}] could not be found!");
                return DbProcessResultEnum.NotFound;
            }
            else if (idols.Count > 1)
            {
                logger.Log($"Idol [{idolName}]-[{(noGroup ? "No group specified" : idolGroup)}] returned multiple results!");
                return DbProcessResultEnum.MultipleResults;
            }

            Idol idol = idols[0];

            User user = await userRepository.FirstOrDefaultAsync(u => u.DiscordId == userId.ToString(), u => u.Idols);
            user ??= new User() { DiscordId = userId.ToString(), Idols = [] };
            List<Idol> userIdols = await idolRepository.GetListByNamesAsync(idolName, idolGroup, userId.ToString());
            if (userIdols.Count > 0)
            {
                logger.Log($"{userId} user's with idol [{idolName}]-[{(noGroup ? "No group specified" : idolGroup)}] is already connected!");
                return userIdols.Count == 1 ? DbProcessResultEnum.AlreadyExists : DbProcessResultEnum.MultipleExists;
            }

            user.Idols.Add(idol);

            await userRepository.UpdateAsync(user);

            logger.Log($"{userId} user's idol [{idolName}]-[{(noGroup ? "No group specified" : idolGroup)}] added successfully!");
            return DbProcessResultEnum.Success;
        }
        catch (Exception ex)
        {
            logger.Error("UserIdolService.cs AddUserIdolAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }

    public async Task<DbProcessResultEnum> AddUserIdolGroupAsync(ulong userId, string biasGroup)
    {
        try
        {
            List<Idol> idols = await idolRepository.GetListAsync(i => i.Group.Name == biasGroup, i => i.Group);

            User user = await userRepository.FirstOrDefaultAsync(u => u.DiscordId == userId.ToString(), u => u.Idols);
            user ??= new User() { DiscordId = userId.ToString(), Idols = [] };

            List<Idol> userIdols = await idolRepository
                .GetListAsync(i =>
                    i.Users.FirstOrDefault(u => u.DiscordId == userId.ToString()) != null
                    && i.Group.Name == biasGroup,
                    i => i.Group,
                    i => i.Users);
            if (userIdols.Count >= idols.Count)
            {
                logger.Log($"{userId} user's with idols from group [{biasGroup}] is already connected!");
                return DbProcessResultEnum.AlreadyExists;
            }

            if (userIdols.Count > 0)
            {
                idols = idols.Except(userIdols).ToList();
            }

            foreach (Idol idol in idols)
            {
                user.Idols.Add(idol);
            }

            await userRepository.UpdateAsync(user);

            logger.Log($"{userId} user's idols from group [{biasGroup}] added successfully!");
            return DbProcessResultEnum.Success;
        }
        catch (Exception ex)
        {
            logger.Error("UserIdolService.cs AddUserIdolGroupAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }

    public async Task<DbProcessResultEnum> ClearUserIdolAsync(ulong userId)
    {
        try
        {
            User user = await userRepository.FirstOrDefaultAsync(u => u.DiscordId == userId.ToString(), u => u.Idols);
            user ??= new User() { DiscordId = userId.ToString() };
            user.Idols = [];

            await userRepository.UpdateAsync(user);

            logger.Log("User idol added successfully!");
            return DbProcessResultEnum.Success;
        }
        catch (Exception ex)
        {
            logger.Error("UserIdolService.cs ClearUserIdolAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }

    public async Task<DbProcessResultEnum> RemoveUserIdolAsync(ulong userId, string idolName, string idolGroup)
    {
        try
        {
            bool noGroup = string.IsNullOrEmpty(idolGroup);

            User user = await userRepository.FirstOrDefaultAsync(u => u.DiscordId == userId.ToString(), u => u.Idols);
            user ??= new User() { DiscordId = userId.ToString() };
            List<Idol> idols = user.Idols.Where(i => i.Name == idolName && (string.IsNullOrEmpty(idolGroup) || idolGroup == i.Group.Name)).ToList();
            if (idols.Count == 0)
            {
                logger.Log($"{userId} user's with idol [{idolName}]-[{(noGroup ? "No group specified" : idolGroup)}] are not connected currently!");
                return DbProcessResultEnum.PartialNotFound;
            }
            else if (idols.Count > 1)
            {
                logger.Log($"Idol [{idolName}]-[{(noGroup ? "No group specified" : idolGroup)}] returned multiple results for {userId} user's idols!");
                return DbProcessResultEnum.MultipleResults;
            }
            Idol idol = idols[0];
            user.Idols.Remove(idol);

            await userRepository.UpdateAsync(user);

            logger.Log($"{userId} user's idol [{idolName}]-[{(noGroup ? "No group specified" : idolGroup)}] removed successfully!");
            return DbProcessResultEnum.Success;
        }
        catch (Exception ex)
        {
            logger.Error("UserIdolService.cs RemoveUserIdolAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }

    public async Task<DbProcessResultEnum> RemoveUserIdolGroupAsync(ulong userId, string biasGroup)
    {
        try
        {
            User user = await userRepository.FirstOrDefaultAsync(u => u.DiscordId == userId.ToString(), u => u.Idols);
            user ??= new User() { DiscordId = userId.ToString() };

            List<Idol> userIdols = await idolRepository
                .GetListAsync(i =>
                    i.Users.FirstOrDefault(u => u.DiscordId == userId.ToString()) != null
                    && i.Group.Name == biasGroup,
                    i => i.Group,
                    i => i.Users);
            if (userIdols.Count == 0)
            {
                logger.Log($"{userId} user's with idols from group [{biasGroup}] are not connected currently!");
                return DbProcessResultEnum.PartialNotFound;
            }

            foreach (Idol idol in userIdols)
            {
                user.Idols.Remove(idol);
            }

            await userRepository.UpdateAsync(user);

            logger.Log($"{userId} user's idols from group [{biasGroup}] removed successfully!");
            return DbProcessResultEnum.Success;
        }
        catch (Exception ex)
        {
            logger.Error("UserIdolService.cs RemoveUserIdolGroupAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }
}

﻿using AutoMapper;
using Discord_Bot.Communication;
using Discord_Bot.Core.Caching;
using Discord_Bot.Core.Logger;
using Discord_Bot.Database.Models;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using Discord_Bot.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices
{
    public class IdolService(IIdolRepository idolRepository, IIdolGroupRepository idolGroupRepository, IUserRepository userRepository, IMapper mapper, Logging logger, Cache cache) : BaseService(mapper, logger, cache), IIdolService
    {
        private readonly IIdolRepository idolRepository = idolRepository;
        private readonly IIdolGroupRepository idolGroupRepository = idolGroupRepository;
        private readonly IUserRepository userRepository = userRepository;

        public async Task<DbProcessResultEnum> AddIdolAsync(string idolName, string idolGroup)
        {
            try
            {
                if (await idolRepository.IdolExistsAsync(idolName, idolGroup))
                {
                    logger.Log($"Idol [{idolName}]-[{idolGroup}] is already in database!");
                    return DbProcessResultEnum.AlreadyExists;
                }

                IdolGroup group = await idolGroupRepository.GetGroupAsync(idolGroup);

                group ??= new()
                {
                    GroupId = 0,
                    Name = idolGroup,
                };

                Idol idol = new()
                {
                    IdolId = 0,
                    Group = group,
                    Name = idolName,
                };
                await idolRepository.AddIdolAsync(idol);

                logger.Log($"Idol [{idolName}]-[{idolGroup}] added successfully!");
                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("IdolService.cs AddIdolAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<DbProcessResultEnum> AddUserIdolAsync(ulong userId, string idolName, string idolGroup)
        {
            try
            {
                List<Idol> idols = await idolRepository.GetIdolsByNameAndGroupAsync(idolName, idolGroup);

                if (idols.Count == 0)
                {
                    logger.Log($"Idol [{idolName}]-[{idolGroup}] could not be found!");
                    return DbProcessResultEnum.NotFound;
                }
                else if (idols.Count > 1)
                {
                    logger.Log($"Idol [{idolName}]-[{idolGroup}] returned multiple results!");
                    return DbProcessResultEnum.MultipleResults;
                }

                Idol idol = idols[0];

                User user = await userRepository.GetUserWithIdolsByDiscordIdAsync(userId);
                List<Idol> userIdols = user.Idols.Where(i => i.Name == idolName && (string.IsNullOrEmpty(idolGroup) || idolGroup == i.Group.Name)).ToList();
                if (userIdols.Count > 0)
                {
                    logger.Log($"{userId} user's with idol [{idolName}]-[{idolGroup}] is already connected an existing connection!");
                    return userIdols.Count == 1 ? DbProcessResultEnum.AlreadyExists : DbProcessResultEnum.MultipleExists;
                }

                user.Idols.Add(idol);

                await userRepository.UpdateUserAsync(user);

                logger.Log($"{userId} user's idol [{idolName}]-[{idolGroup}] added successfully!");
                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("IdolService.cs AddUserIdolAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<DbProcessResultEnum> ClearUserIdolAsync(ulong userId)
        {
            try
            {
                User user = await userRepository.GetUserWithIdolsByDiscordIdAsync(userId);

                user.Idols = [];

                await userRepository.UpdateUserAsync(user);

                logger.Log("User idol added successfully!");
                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("IdolService.cs ClearUserIdolAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<List<IdolResource>> GetIdolsByGroupAsync(string groupName)
        {
            List<IdolResource> result = null;
            try
            {
                List<Idol> idols = await idolRepository.GetIdolsByGroupAsync(groupName);

                result = mapper.Map<List<Idol>, List<IdolResource>>(idols);
            }
            catch (Exception ex)
            {
                logger.Error("IdolService.cs GetIdolsByGroupAsync", ex.ToString());
            }
            return result;
        }

        public async Task<ListWithDbResult<UserResource>> GetUsersWithIdolsAsync(string[] nameList)
        {
            ListWithDbResult<UserResource> result = new(null, DbProcessResultEnum.Failure);
            try
            {
                List<Idol> idols = await idolRepository.GetIdolsByNamesAsync(nameList);

                if (CollectionTools.IsNullOrEmpty(idols))
                {
                    result.ProcessResultEnum = DbProcessResultEnum.NotFound;
                    return result;
                }

                List<User> users = idols.SelectMany(i => i.Users).DistinctBy(u => u.UserId).ToList();

                result.List = mapper.Map<List<User>, List<UserResource>>(users);
                result.ProcessResultEnum = DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("IdolService.cs GetUsersWithIdolsAsync", ex.ToString());
            }
            return result;
        }

        public async Task<DbProcessResultEnum> RemoveIdolAsync(string idolName, string idolGroup)
        {
            try
            {
                Idol idol = await idolRepository.GetIdolByNameAndGroupAsync(idolName, idolGroup);
                if (idol != null)
                {
                    await idolRepository.RemoveIdolAsync(idol);

                    logger.Log($"Idol [{idolName}]-[{idolGroup}] removed successfully!");
                    return DbProcessResultEnum.Success;
                }
                else
                {
                    logger.Log($"Idol [{idolName}]-[{idolGroup}] could not be found!");
                    return DbProcessResultEnum.NotFound;
                }
            }
            catch (Exception ex)
            {
                logger.Error("IdolService.cs RemoveIdolAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<DbProcessResultEnum> RemoveUserIdolAsync(ulong userId, string idolName, string idolGroup)
        {
            try
            {
                User user = await userRepository.GetUserWithIdolsByDiscordIdAsync(userId);
                List<Idol> idols = user.Idols.Where(i => i.Name == idolName && (string.IsNullOrEmpty(idolGroup) || idolGroup == i.Group.Name)).ToList();
                if (idols.Count == 0)
                {
                    logger.Log($"{userId} user's with idol [{idolName}]-[{idolGroup}] are not connected currently!");
                    return DbProcessResultEnum.PartialNotFound;
                }
                else if (idols.Count > 1)
                {
                    logger.Log($"Idol [{idolName}]-[{idolGroup}] returned multiple results for {userId} user's idols!");
                    return DbProcessResultEnum.MultipleResults;
                }

                Idol idol = idols[0];

                user.Idols.Remove(idol);

                await userRepository.UpdateUserAsync(user);

                logger.Log($"{userId} user's idol [{idolName}]-[{idolGroup}] removed successfully!");
                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("IdolService.cs RemoveUserIdolAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<List<IdolResource>> GetUserIdolsListAsync(ulong userId, string groupName)
        {
            List<IdolResource> result = null;
            try
            {
                List<Idol> idols = await idolRepository.GetUserIdolsListAsync(userId, groupName);

                result = mapper.Map<List<Idol>, List<IdolResource>>(idols);
            }
            catch (Exception ex)
            {
                logger.Error("IdolService.cs GetUserIdolsListAsync", ex.ToString());
            }
            return result;
        }
    }
}
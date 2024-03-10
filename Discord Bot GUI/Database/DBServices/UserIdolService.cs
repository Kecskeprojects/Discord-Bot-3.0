using AutoMapper;
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

namespace Discord_Bot.Database.DBServices
{
    public class UserIdolService(
        IIdolRepository idolRepository,
        IUserRepository userRepository,
        IMapper mapper,
        Logging logger,
        Cache cache) : BaseService(mapper, logger, cache), IUserIdolService
    {
        private readonly IIdolRepository idolRepository = idolRepository;
        private readonly IUserRepository userRepository = userRepository;

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
                logger.Error("UserIdolService.cs GetUserIdolsListAsync", ex.ToString());
            }
            return result;
        }

        public async Task<DbProcessResultEnum> AddUserIdolAsync(ulong userId, string idolName, string idolGroup)
        {
            try
            {
                bool noGroup = string.IsNullOrEmpty(idolGroup);

                List<Idol> idols = await idolRepository.GetIdolsByNameAndGroupAsync(idolName, idolGroup);

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

                User user = await userRepository.GetUserWithIdolsByDiscordIdAsync(userId);
                user ??= new User() { DiscordId = userId.ToString(), Idols = [] };
                List<Idol> userIdols = await idolRepository.GetIdolsByNameAndGroupAndUserAsync(userId, idolName, idolGroup);
                if (userIdols.Count > 0)
                {
                    logger.Log($"{userId} user's with idol [{idolName}]-[{(noGroup ? "No group specified" : idolGroup)}] is already connected!");
                    return userIdols.Count == 1 ? DbProcessResultEnum.AlreadyExists : DbProcessResultEnum.MultipleExists;
                }

                user.Idols.Add(idol);

                await userRepository.UpdateUserAsync(user);

                logger.Log($"{userId} user's idol [{idolName}]-[{(noGroup ? "No group specified" : idolGroup)}] added successfully!");
                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("UserIdolService.cs AddUserIdolAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<DbProcessResultEnum> ClearUserIdolAsync(ulong userId)
        {
            try
            {
                User user = await userRepository.GetUserWithIdolsByDiscordIdAsync(userId);
                user ??= new User() { DiscordId = userId.ToString() };
                user.Idols = [];

                await userRepository.UpdateUserAsync(user);

                logger.Log("User idol added successfully!");
                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("UserIdolService.cs ClearUserIdolAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<DbProcessResultEnum> RemoveUserIdolAsync(ulong userId, string idolName, string idolGroup)
        {
            try
            {
                bool noGroup = string.IsNullOrEmpty(idolGroup);

                User user = await userRepository.GetUserWithIdolsByDiscordIdAsync(userId);
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

                await userRepository.UpdateUserAsync(user);

                logger.Log($"{userId} user's idol [{idolName}]-[{(noGroup ? "No group specified" : idolGroup)}] removed successfully!");
                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("UserIdolService.cs RemoveUserIdolAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }
    }
}

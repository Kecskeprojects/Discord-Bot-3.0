using AutoMapper;
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

        public async Task<DbProcessResultEnum> AddBiasAsync(string biasName, string biasGroup)
        {
            try
            {
                if (await idolRepository.IdolExistsAsync(biasName, biasGroup))
                {
                    logger.Log($"Idol [{biasName}]-[{biasGroup}] is already in database!");
                    return DbProcessResultEnum.AlreadyExists;
                }

                IdolGroup group = await idolGroupRepository.GetGroupAsync(biasGroup);

                group ??= new()
                {
                    GroupId = 0,
                    Name = biasGroup,
                };

                Idol idol = new()
                {
                    IdolId = 0,
                    Group = group,
                    Name = biasName,
                };
                await idolRepository.AddBiasAsync(idol);

                logger.Log($"Idol [{biasName}]-[{biasGroup}] added successfully!");
                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("IdolService.cs AddBiasAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<DbProcessResultEnum> AddUserBiasAsync(ulong userId, string biasName, string biasGroup)
        {
            try
            {
                List<Idol> idols = await idolRepository.GetBiasesByNameAndGroupAsync(biasName, biasGroup);

                if (idols.Count == 0)
                {
                    logger.Log($"Idol [{biasName}]-[{biasGroup}] could not be found!");
                    return DbProcessResultEnum.NotFound;
                }
                else if (idols.Count > 1)
                {
                    logger.Log($"Idol [{biasName}]-[{biasGroup}] returned multiple results!");
                    return DbProcessResultEnum.MultipleResults;
                }

                Idol idol = idols[0];

                User user = await userRepository.GetUserWithBiasesByDiscordIdAsync(userId);
                List<Idol> userBiases = user.Idols.Where(i => i.Name == biasName && (string.IsNullOrEmpty(biasGroup) || biasGroup == i.Group.Name)).ToList();
                if (userBiases.Count > 0)
                {
                    logger.Log($"{userId} user's with bias [{biasName}]-[{biasGroup}] is already connected an existing connection!");
                    return userBiases.Count == 1 ? DbProcessResultEnum.AlreadyExists : DbProcessResultEnum.MultipleExists;
                }

                user.Idols.Add(idol);

                await userRepository.UpdateUserAsync(user);

                logger.Log($"{userId} user's bias [{biasName}]-[{biasGroup}] added successfully!");
                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("IdolService.cs AddUserBiasAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<DbProcessResultEnum> ClearUserBiasAsync(ulong userId)
        {
            try
            {
                User user = await userRepository.GetUserWithBiasesByDiscordIdAsync(userId);

                user.Idols = [];

                await userRepository.UpdateUserAsync(user);

                logger.Log("User bias added successfully!");
                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("IdolService.cs ClearUserBiasAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<List<IdolResource>> GetBiasesByGroupAsync(string groupName)
        {
            List<IdolResource> result = null;
            try
            {
                List<Idol> idols = await idolRepository.GetBiasesByGroupAsync(groupName);

                result = mapper.Map<List<Idol>, List<IdolResource>>(idols);
            }
            catch (Exception ex)
            {
                logger.Error("IdolService.cs GetBiasesByGroupAsync", ex.ToString());
            }
            return result;
        }

        public async Task<ListWithDbResult<UserResource>> GetUsersWithBiasesAsync(string[] nameList)
        {
            ListWithDbResult<UserResource> result = new(null, DbProcessResultEnum.Failure);
            try
            {
                List<Idol> idols = await idolRepository.GetBiasesByNamesAsync(nameList);

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
                logger.Error("IdolService.cs GetUsersWithBiasesAsync", ex.ToString());
            }
            return result;
        }

        public async Task<DbProcessResultEnum> RemoveBiasAsync(string biasName, string biasGroup)
        {
            try
            {
                Idol idol = await idolRepository.GetBiasByNameAndGroupAsync(biasName, biasGroup);
                if (idol != null)
                {
                    await idolRepository.RemoveIdolAsync(idol);

                    logger.Log($"Idol [{biasName}]-[{biasGroup}] removed successfully!");
                    return DbProcessResultEnum.Success;
                }
                else
                {
                    logger.Log($"Idol [{biasName}]-[{biasGroup}] could not be found!");
                    return DbProcessResultEnum.NotFound;
                }
            }
            catch (Exception ex)
            {
                logger.Error("IdolService.cs RemoveBiasAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<DbProcessResultEnum> RemoveUserBiasAsync(ulong userId, string biasName, string biasGroup)
        {
            try
            {
                User user = await userRepository.GetUserWithBiasesByDiscordIdAsync(userId);
                List<Idol> idols = user.Idols.Where(i => i.Name == biasName && (string.IsNullOrEmpty(biasGroup) || biasGroup == i.Group.Name)).ToList();
                if (idols.Count == 0)
                {
                    logger.Log($"{userId} user's with bias [{biasName}]-[{biasGroup}] are not connected currently!");
                    return DbProcessResultEnum.PartialNotFound;
                }
                else if (idols.Count > 1)
                {
                    logger.Log($"Idol [{biasName}]-[{biasGroup}] returned multiple results for {userId} user's biases!");
                    return DbProcessResultEnum.MultipleResults;
                }

                Idol idol = idols[0];

                user.Idols.Remove(idol);

                await userRepository.UpdateUserAsync(user);

                logger.Log($"{userId} user's bias [{biasName}]-[{biasGroup}] removed successfully!");
                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("IdolService.cs RemoveUserBiasAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<List<IdolResource>> GetUserBiasesListAsync(ulong userId, string groupName)
        {
            List<IdolResource> result = null;
            try
            {
                List<Idol> idols = await idolRepository.GetUserBiasesListAsync(userId, groupName);

                result = mapper.Map<List<Idol>, List<IdolResource>>(idols);
            }
            catch (Exception ex)
            {
                logger.Error("IdolService.cs GetUserBiasesListAsync", ex.ToString());
            }
            return result;
        }
    }
}

using AutoMapper;
using Discord_Bot.Communication;
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
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices
{
    public class UserService(IUserRepository userRepository, IIdolRepository idolRepository, IMapper mapper, Logging logger, Cache cache) : BaseService(mapper, logger, cache), IUserService
    {
        private readonly IUserRepository userRepository = userRepository;
        private readonly IIdolRepository idolRepository = idolRepository;

        public async Task<DbProcessResultEnum> AddLastfmUsernameAsync(ulong userId, string name)
        {
            try
            {
                User user = await userRepository.GetUserByDiscordIdAsync(userId);

                user ??= new User() { DiscordId = userId.ToString() };

                if (!string.IsNullOrEmpty(user.LastFmusername))
                {
                    return DbProcessResultEnum.AlreadyExists;
                }
                user.LastFmusername = name;
                await userRepository.UpdateUserAsync(user);

                logger.Log("Lastfm username added successfully!");
                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("UserService.cs AddLastfmUsernameAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<List<UserResource>> GetAllLastFmUsersAsync()
        {
            List<UserResource> result = null;
            try
            {
                List<User> users = await userRepository.GetAllLastFmUsersAsync();
                result = mapper.Map<List<User>, List<UserResource>>(users);
            }
            catch (Exception ex)
            {
                logger.Error("UserService.cs GetAllLastFmUsersAsync", ex.ToString());
            }
            return result;
        }

        public async Task<UserResource> GetUserAsync(ulong userId)
        {
            UserResource result = null;
            try
            {
                User user = await userRepository.GetUserByDiscordIdAsync(userId);
                result = mapper.Map<User, UserResource>(user);
            }
            catch (Exception ex)
            {
                logger.Error("UserService.cs GetUserAsync", ex.ToString());
            }
            return result;
        }

        public async Task<ListWithDbResult<UserResource>> GetUsersWithIdolsAsync(string[] nameList)
        {
            ListWithDbResult<UserResource> result = new(null, DbProcessResultEnum.Failure);
            try
            {
                List<Idol> idols = [];
                foreach (string item in nameList)
                {
                    string[] parts = item.Split("-");
                    string idolOrGroupName = parts[0];
                    string groupName = parts.Length == 2 ? parts[1] : "";
                    idols = idols.Union(await idolRepository.GetIdolsByNameAndGroupAsync(idolOrGroupName, groupName)).ToList();
                }

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
                logger.Error("UserService.cs GetUsersWithIdolsAsync", ex.ToString());
            }
            return result;
        }

        public async Task<DbProcessResultEnum> RemoveLastfmUsernameAsync(ulong userId)
        {
            try
            {
                User user = await userRepository.GetUserByDiscordIdAsync(userId);

                if (user != null || string.IsNullOrEmpty(user.LastFmusername))
                {
                    return DbProcessResultEnum.NotFound;
                }
                user.LastFmusername = null;
                await userRepository.UpdateUserAsync(user);

                logger.Log("Lastfm username added successfully!");
                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("UserService.cs RemoveLastfmUsernameAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }
    }
}

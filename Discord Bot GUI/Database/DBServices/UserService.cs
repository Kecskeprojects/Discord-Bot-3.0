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
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices
{
    public class UserService(
        IUserRepository userRepository,
        IMapper mapper,
        Logging logger,
        Cache cache) : BaseService(mapper, logger, cache), IUserService
    {
        private readonly IUserRepository userRepository = userRepository;

        public async Task<DbProcessResultEnum> AddLastfmUsernameAsync(ulong userId, string name)
        {
            try
            {
                User user = await userRepository.FirstOrDefaultAsync(u => u.DiscordId == userId.ToString());

                user ??= new User() { DiscordId = userId.ToString() };

                if (!string.IsNullOrEmpty(user.LastFmusername))
                {
                    return DbProcessResultEnum.AlreadyExists;
                }
                user.LastFmusername = name;
                await userRepository.UpdateAsync(user);

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
                List<User> users = await userRepository.GetAllAsync();
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
                User user = await userRepository.FirstOrDefaultAsync(u => u.DiscordId == userId.ToString());
                result = mapper.Map<User, UserResource>(user);
            }
            catch (Exception ex)
            {
                logger.Error("UserService.cs GetUserAsync", ex.ToString());
            }
            return result;
        }

        public async Task<DbProcessResultEnum> RemoveLastfmUsernameAsync(ulong userId)
        {
            try
            {
                User user = await userRepository.FirstOrDefaultAsync(u => u.DiscordId == userId.ToString());

                if (user != null || string.IsNullOrEmpty(user.LastFmusername))
                {
                    return DbProcessResultEnum.NotFound;
                }
                user.LastFmusername = null;
                await userRepository.SaveChangesAsync();

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

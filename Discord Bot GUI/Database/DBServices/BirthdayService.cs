﻿using AutoMapper;
using Discord_Bot.Core.Caching;
using Discord_Bot.Core.Logger;
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
    public class BirthdayService : BaseService, IBirthdayService
    {
        private readonly IServerRepository serverRepository;
        private readonly IUserRepository userRepository;
        private readonly IBirthdayRepository birthdayRepository;

        public BirthdayService(IServerRepository serverRepository, IUserRepository userRepository, IBirthdayRepository birthdayRepository, IMapper mapper, Logging logger, Cache cache) : base(mapper, logger, cache)
        {
            this.serverRepository = serverRepository;
            this.userRepository = userRepository;
            this.birthdayRepository = birthdayRepository;
        }

        public async Task<DbProcessResultEnum> AddBirthdayAsync(ulong serverId, ulong userId, DateTime date)
        {
            try
            {
                Birthday birthday = await birthdayRepository.GetBirthdayAsync(serverId, userId);

                if(birthday != null)
                {
                    if(birthday.Date != date)
                    {
                        birthday.Date = date;
                        await birthdayRepository.UpdateBirthdayAsync(birthday);
                        logger.Log("Birthday updated successfully!");
                        return DbProcessResultEnum.UpdatedExisting;
                    }
                    else
                    {
                        return DbProcessResultEnum.AlreadyExists;
                    }
                }
                Server server = await serverRepository.GetByDiscordIdAsync(serverId);
                User user = await userRepository.GetUserByDiscordId(userId);

                birthday = new()
                {
                    BirthdayId = 0,
                    Date = date,
                    User = user ?? new User() { DiscordId = userId.ToString() },
                    Server = server ?? new Server() { DiscordId = serverId.ToString() },
                };
                await birthdayRepository.AddBirthdayAsync(birthday);

                logger.Log("Birthday added successfully!");
                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("BirthdayService.cs AddBirthdayAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<List<BirthdayResource>> GetBirthdaysByDateAsync(DateTime dateTime)
        {
            List<Birthday> birthday = await birthdayRepository.GetBirthdaysByDateAsync(dateTime);
            return mapper.Map<List<BirthdayResource>>(birthday);
        }

        public async Task<DbProcessResultEnum> RemoveBirthdayAsync(ulong serverId, ulong userId)
        {
            try
            {
                Birthday birthday = await birthdayRepository.GetBirthdayAsync(serverId, userId);
                if (birthday != null)
                {
                    await birthdayRepository.RemoveBirthdayAsync(birthday);

                    logger.Log("Birthday removed successfully!");
                    return DbProcessResultEnum.Success;
                }
                else
                {
                    logger.Log("Birthday not found!");
                    return DbProcessResultEnum.NotFound;
                }
            }
            catch (Exception ex)
            {
                logger.Error("BirthdayService.cs RemoveBirthdayAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }
    }
}

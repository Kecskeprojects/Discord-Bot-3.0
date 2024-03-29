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
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices
{
    public class BirthdayService(IServerRepository serverRepository, IUserRepository userRepository, IBirthdayRepository birthdayRepository, IMapper mapper, Logging logger, Cache cache) : BaseService(mapper, logger, cache), IBirthdayService
    {
        private readonly IServerRepository serverRepository = serverRepository;
        private readonly IUserRepository userRepository = userRepository;
        private readonly IBirthdayRepository birthdayRepository = birthdayRepository;

        public async Task<DbProcessResultEnum> AddBirthdayAsync(ulong serverId, ulong userId, DateTime date)
        {
            try
            {
                Birthday birthday = await birthdayRepository.GetBirthdayAsync(serverId, userId);
                DateOnly datePart = DateOnly.FromDateTime(date);

                if (birthday != null)
                {
                    if (birthday.Date != datePart)
                    {
                        birthday.Date = datePart;
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
                User user = await userRepository.GetUserByDiscordIdAsync(userId);

                birthday = new()
                {
                    BirthdayId = 0,
                    Date = datePart,
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

        public async Task<List<BirthdayResource>> GetBirthdaysByDateAsync()
        {
            List<Birthday> birthday = await birthdayRepository.GetBirthdaysByDateAsync();
            return mapper.Map<List<Birthday>, List<BirthdayResource>>(birthday);
        }

        public async Task<List<BirthdayResource>> GetServerBirthdayListAsync(ulong serverId)
        {
            List<Birthday> birthday = await birthdayRepository.GetBirthdaysByServerAsync(serverId);
            return mapper.Map<List<Birthday>, List<BirthdayResource>>(birthday);
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

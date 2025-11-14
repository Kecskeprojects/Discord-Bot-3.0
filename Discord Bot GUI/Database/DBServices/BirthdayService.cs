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

namespace Discord_Bot.Database.DBServices;

public class BirthdayService(
    IServerRepository serverRepository,
    IUserRepository userRepository,
    IBirthdayRepository birthdayRepository,
    IMapper mapper,
    BotLogger logger,
    ServerCache cache) : BaseService(mapper, logger, cache), IBirthdayService
{
    private readonly IServerRepository serverRepository = serverRepository;
    private readonly IUserRepository userRepository = userRepository;
    private readonly IBirthdayRepository birthdayRepository = birthdayRepository;

    public async Task<DbProcessResultEnum> AddBirthdayAsync(ulong serverId, ulong userId, DateTime date)
    {
        try
        {
            Birthday birthday = await birthdayRepository.FirstOrDefaultAsync(
                b => b.Server.DiscordId == serverId.ToString()
                && b.User.DiscordId == userId.ToString(),
                b => b.Server);
            DateOnly datePart = DateOnly.FromDateTime(date);

            if (birthday != null)
            {
                if (birthday.Date != datePart)
                {
                    birthday.Date = datePart;
                    _ = await birthdayRepository.SaveChangesAsync();
                    logger.Log("Birthday updated successfully!");
                    return DbProcessResultEnum.UpdatedExisting;
                }
                else
                {
                    return DbProcessResultEnum.AlreadyExists;
                }
            }
            Server server = await serverRepository.FirstOrDefaultAsync(s => s.DiscordId == serverId.ToString());
            User user = await userRepository.FirstOrDefaultAsync(u => u.DiscordId == userId.ToString());

            birthday = new()
            {
                BirthdayId = 0,
                Date = datePart,
                User = user ?? new User() { DiscordId = userId.ToString() },
                Server = server ?? new Server() { DiscordId = serverId.ToString() },
            };
            _ = await birthdayRepository.AddAsync(birthday);

            logger.Log("Birthday added successfully!");
            return DbProcessResultEnum.Success;
        }
        catch (Exception ex)
        {
            logger.Error("BirthdayService.cs AddBirthdayAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }

    public async Task<List<BirthdayResource>> GetBirthdaysByDateAsync()
    {
        List<Birthday> birthday = [];
        try
        {
            birthday = await birthdayRepository.GetListAsync(
                b => b.Date.Month == DateTime.UtcNow.Month
                && b.Date.Day == DateTime.UtcNow.Day,
                b => b.Server,
                b => b.User);
        }
        catch (Exception ex)
        {
            logger.Error("BirthdayService.cs GetBirthdaysByDateAsync", ex);
        }
        return mapper.Map<List<Birthday>, List<BirthdayResource>>(birthday);
    }

    public async Task<List<BirthdayResource>> GetServerBirthdayListAsync(ulong serverId)
    {
        List<Birthday> birthday = [];
        try
        {
            birthday = await birthdayRepository.GetListForServerAsync(serverId.ToString());
        }
        catch (Exception ex)
        {
            logger.Error("BirthdayService.cs GetServerBirthdayListAsync", ex);
        }
        return mapper.Map<List<Birthday>, List<BirthdayResource>>(birthday);
    }

    public async Task<DbProcessResultEnum> RemoveBirthdayAsync(ulong serverId, ulong userId)
    {
        try
        {
            Birthday birthday = await birthdayRepository.FirstOrDefaultAsync(
                b => b.Server.DiscordId == serverId.ToString()
                && b.User.DiscordId == userId.ToString(),
                b => b.Server);
            if (birthday != null)
            {
                _ = await birthdayRepository.RemoveAsync(birthday);

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
            logger.Error("BirthdayService.cs RemoveBirthdayAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }
}

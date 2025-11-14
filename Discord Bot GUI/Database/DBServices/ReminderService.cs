using AutoMapper;
using Discord_Bot.Core;
using Discord_Bot.Core.Caching;
using Discord_Bot.Database.Models;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using Discord_Bot.Tools.NativeTools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices;

public class ReminderService(
    IReminderRepository reminderRepository,
    IUserRepository userRepository,
    IMapper mapper,
    BotLogger logger,
    ServerCache cache) : BaseService(mapper, logger, cache), IReminderService
{
    private readonly IReminderRepository reminderRepository = reminderRepository;
    private readonly IUserRepository userRepository = userRepository;

    public async Task<DbProcessResultEnum> AddReminderAsync(ulong userId, DateTime date, string remindMessage)
    {
        try
        {
            User user = await userRepository.FirstOrDefaultAsync(u => u.DiscordId == userId.ToString());
            Reminder reminder = new()
            {
                ReminderId = 0,
                User = user ?? new User() { DiscordId = userId.ToString() },
                Date = date,
                Message = remindMessage
            };
            _ = await reminderRepository.AddAsync(reminder);

            logger.Log($"Reminder added for the following user: {userId}\nwith the following date: {date:yyyy.MM.dd HH:mm:ss}");
            return DbProcessResultEnum.Success;
        }
        catch (Exception ex)
        {
            logger.Error("ReminderService.cs GetCurrentReminderAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }

    public async Task<List<ReminderResource>> GetCurrentRemindersAsync(DateTime dateTime)
    {
        List<ReminderResource> result = null;
        try
        {
            List<Reminder> reminders = await reminderRepository.GetListAsync(r => r.Date <= dateTime, r => r.User);
            if (reminders == null)
            {
                return null;
            }

            result = mapper.Map<List<Reminder>, List<ReminderResource>>(reminders);
        }
        catch (Exception ex)
        {
            logger.Error("ReminderService.cs GetCurrentReminderAsync", ex);
        }

        return result;
    }

    public async Task<List<ReminderResource>> GetUserReminderListAsync(ulong userId)
    {
        List<ReminderResource> result = null;
        try
        {
            List<Reminder> server = await reminderRepository.GetListAsync(r =>
            r.User.DiscordId == userId.ToString(),
            orderBy: r => r.Date,
            ascending: true,
            includes: r => r.User);

            if (server == null)
            {
                return null;
            }

            result = mapper.Map<List<Reminder>, List<ReminderResource>>(server);
        }
        catch (Exception ex)
        {
            logger.Error("ReminderService.cs GetCurrentReminderAsync", ex);
        }

        return result;
    }

    public async Task<DbProcessResultEnum> RemoveCurrentRemindersAsync(List<int> reminderIds)
    {
        try
        {
            List<Reminder> reminders = await reminderRepository.GetListAsync(r => reminderIds.Contains(r.ReminderId));

            if (!CollectionTools.IsNullOrEmpty(reminders))
            {
                _ = await reminderRepository.RemoveAsync(reminders);

                logger.Log($"Reminders removed with the following IDs: {string.Join(",", reminderIds)}");
            }
            else
            {
                logger.Log("No reminders to remove currently.");
            }
            return DbProcessResultEnum.Success;
        }
        catch (Exception ex)
        {
            logger.Error("ReminderService.cs RemoveCurrentReminderAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }

    public async Task<DbProcessResultEnum> RemoveUserReminderAsync(ulong userId, int reminderOrderId)
    {
        try
        {
            Reminder reminder = await reminderRepository.GetByIndexAsync(userId.ToString(), reminderOrderId);

            if (reminder != null)
            {
                _ = await reminderRepository.RemoveAsync(reminder);

                logger.Log($"Reminders removed by the following user: {userId}\nwith the following ID: {reminderOrderId}");
                return DbProcessResultEnum.Success;
            }
            else
            {
                logger.Log($"Reminder was not found with the following user: {userId}\nand reminder ID: {reminderOrderId}");
                return DbProcessResultEnum.NotFound;
            }
        }
        catch (Exception ex)
        {
            logger.Error("ReminderService.cs RemoveCurrentReminderAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }
}

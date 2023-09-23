using AutoMapper;
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
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices
{
    public class ReminderService : BaseService, IReminderService
    {
        private readonly IReminderRepository reminderRepository;
        private readonly IUserRepository userRepository;

        public ReminderService(IMapper mapper, Logging logger, Cache cache, IReminderRepository reminderRepository, IUserRepository userRepository) : base(mapper, logger, cache)
        {
            this.reminderRepository = reminderRepository;
            this.userRepository = userRepository;
        }

        public async Task<DbProcessResultEnum> AddReminderAsync(ulong userId, DateTime date, string remindMessage)
        {
            try
            {
                User user = await userRepository.GetUserByDiscordId(userId);
                Reminder reminder = new()
                {
                    ReminderId = 0,
                    User = user,
                    Date = date,
                    Message = remindMessage
                };
                await reminderRepository.AddReminderAsync(reminder);

                logger.Log($"Reminder added for the following user: {userId}\nwith the following date: {date:yyyy.MM.dd HH:mm:ss}");
                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("ReminderService.cs GetCurrentReminderAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<List<ReminderResource>> GetCurrentRemindersAsync(DateTime dateTime)
        {
            List<ReminderResource> result = null;
            try
            {
                List<Reminder> reminders = await reminderRepository.GetCurrentRemindersAsync(dateTime);
                if (reminders == null) return null;

                result = mapper.Map<List<Reminder>, List<ReminderResource>>(reminders);
            }
            catch (Exception ex)
            {
                logger.Error("ReminderService.cs GetCurrentReminderAsync", ex.ToString());
            }

            return result;
        }

        public async Task<List<ReminderResource>> GetUserReminderListAsync(ulong userId)
        {
            List<ReminderResource> result = null;
            try
            {
                List<Reminder> server = await reminderRepository.GetUserReminderListAsync(userId);
                if (server == null) return null;

                result = mapper.Map<List<Reminder>, List<ReminderResource>>(server);
            }
            catch (Exception ex)
            {
                logger.Error("ReminderService.cs GetCurrentReminderAsync", ex.ToString());
            }

            return result;
        }

        public async Task<DbProcessResultEnum> RemoveCurrentRemindersAsync(List<int> reminderIds)
        {
            try
            {
                List<Reminder> reminders = await reminderRepository.GetRemindersByIds(reminderIds);

                if (CollectionTools.IsNullOrEmpty(reminders))
                {
                    await reminderRepository.RemoveCurrentRemindersAsync(reminders);

                    logger.Log($"Reminders removed with the following IDs: {string.Join(",", reminderIds)}");
                }
                else
                {
                    logger.Log("No reminders to remove currently");
                }
                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("ReminderService.cs RemoveCurrentReminderAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<DbProcessResultEnum> RemoveUserReminderAsync(ulong userId, int reminderId)
        {
            try
            {
                Reminder reminder = await reminderRepository.GetUserReminderById(userId, reminderId);

                if (reminder != null)
                {
                    await reminderRepository.RemoveReminderAsync(reminder);

                    logger.Log($"Reminders removed by the following user: {userId}\nwith the following ID: {reminderId}");
                    return DbProcessResultEnum.Success;
                }
                else
                {
                    logger.Log($"Reminder was not found with the following user: {userId}\nand reminder ID: {reminderId}");
                    return DbProcessResultEnum.NotFound;
                }
            }
            catch (Exception ex)
            {
                logger.Error("ReminderService.cs RemoveCurrentReminderAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }
    }
}

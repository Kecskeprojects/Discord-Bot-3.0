using AutoMapper;
using Discord_Bot.Core.Caching;
using Discord_Bot.Core.Logger;
using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices
{
    public class ReminderService : BaseService, IReminderService
    {
        private readonly IReminderRepository reminderRepository;
        public ReminderService(IMapper mapper, Logging logger, Cache cache, IReminderRepository reminderRepository) : base(mapper, logger, cache) => this.reminderRepository = reminderRepository;

        public async Task<List<ReminderResource>> GetCurrentRemindersAsync(DateTime dateTime)
        {
            List<ReminderResource> result = null;
            try
            {
                List<Reminder> server = await reminderRepository.GetCurrentRemindersAsync(dateTime);
                if (server == null) return null;

                result = mapper.Map<List<Reminder>, List<ReminderResource>>(server);
            }
            catch (Exception ex)
            {
                logger.Error("ReminderService.cs GetCurrentReminderAsync", ex.ToString());
            }

            return result;
        }

        public async Task RemoveCurrentRemindersAsync(List<int> reminderIds)
        {
            try
            {
                List<Reminder> reminders = await reminderRepository.GetRemindersByIds(reminderIds);

                await reminderRepository.RemoveCurrentRemindersAsync(reminders);
            }
            catch (Exception ex)
            {
                logger.Error("ReminderService.cs RemoveCurrentReminderAsync", ex.ToString());
            }
        }
    }
}

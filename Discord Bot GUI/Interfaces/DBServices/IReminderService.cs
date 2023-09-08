using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices
{
    public interface IReminderService
    {
        Task<DbProcessResultEnum> AddReminderAsync(ulong userId, DateTime date, string remindMessage);
        Task<List<ReminderResource>> GetCurrentRemindersAsync(DateTime dateTime);
        Task<List<ReminderResource>> GetUserReminderListAsync(ulong userId);
        Task<DbProcessResultEnum> RemoveCurrentRemindersAsync(List<int> reminderIds);
        Task<DbProcessResultEnum> RemoveUserReminderAsync(ulong userId, int reminderId);
    }
}

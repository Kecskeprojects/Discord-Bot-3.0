using Discord_Bot.Database.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBRepositories
{
    public interface IReminderRepository
    {
        Task AddReminderAsync(Reminder reminder);
        Task<List<Reminder>> GetCurrentRemindersAsync(DateTime dateTime);
        Task<List<Reminder>> GetRemindersByIds(List<int> reminderIds);
        Task<Reminder> GetUserReminderById(ulong userId, int reminderId);
        Task<List<Reminder>> GetUserReminderListAsync(ulong userId);
        Task RemoveCurrentRemindersAsync(List<Reminder> reminders);
        Task RemoveReminderAsync(Reminder reminder);
    }
}

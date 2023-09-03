using Discord_Bot.Database.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBRepositories
{
    public interface IReminderRepository
    {
        Task<List<Reminder>> GetCurrentRemindersAsync(DateTime dateTime);
        Task<List<Reminder>> GetRemindersByIds(List<int> reminderIds);
        Task RemoveCurrentRemindersAsync(List<Reminder> reminders);
    }
}

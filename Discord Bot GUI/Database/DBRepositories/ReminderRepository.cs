using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories
{
    public class ReminderRepository : BaseRepository, IReminderRepository
    {
        public ReminderRepository(MainDbContext context) : base(context)
        {
        }

        public Task<List<Reminder>> GetCurrentRemindersAsync(DateTime dateTime)
        {
            return context.Reminders
                .Include(r => r.User)
                .Where(r => r.Date <= dateTime)
                .ToListAsync();
        }

        public Task<List<Reminder>> GetRemindersByIds(List<int> reminderIds)
        {
            return context.Reminders
                .Where(r => reminderIds.Contains(r.ReminderId))
                .ToListAsync();
        }

        public async Task RemoveCurrentRemindersAsync(List<Reminder> reminders)
        {
            context.Reminders.RemoveRange(reminders);
            await context.SaveChangesAsync();
        }
    }
}

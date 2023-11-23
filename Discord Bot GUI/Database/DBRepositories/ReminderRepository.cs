using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories
{
    public class ReminderRepository(MainDbContext context) : BaseRepository(context), IReminderRepository
    {
        public async Task AddReminderAsync(Reminder reminder)
        {
            context.Reminders.Add(reminder);
            await context.SaveChangesAsync();
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

        public Task<Reminder> GetUserReminderById(ulong userId, int reminderOrderId)
        {
            return context.Reminders
                .Include(r => r.User)
                .Where(r => r.User.DiscordId == userId.ToString())
                .OrderBy(r => r.Date)
                .Skip(reminderOrderId - 1)
                .FirstAsync();
        }

        public Task<List<Reminder>> GetUserReminderListAsync(ulong userId)
        {
            return context.Reminders
                .Include(r => r.User)
                .Where(r => r.User.DiscordId == userId.ToString())
                .OrderBy(r => r.Date)
                .ToListAsync();
        }

        public async Task RemoveCurrentRemindersAsync(List<Reminder> reminders)
        {
            context.Reminders.RemoveRange(reminders);
            await context.SaveChangesAsync();
        }

        public async Task RemoveReminderAsync(Reminder reminder)
        {
            context.Remove(reminder);
            await context.SaveChangesAsync();
        }
    }
}

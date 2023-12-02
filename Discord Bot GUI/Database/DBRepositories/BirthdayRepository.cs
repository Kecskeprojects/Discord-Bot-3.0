using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories
{
    public class BirthdayRepository(MainDbContext context) : BaseRepository(context), IBirthdayRepository
    {
        public async Task AddBirthdayAsync(Birthday birthday)
        {
            context.Birthdays.Add(birthday);
            await context.SaveChangesAsync();
        }

        public Task<Birthday> GetBirthdayAsync(ulong serverId, ulong userId)
        {
            return context.Birthdays
                .FirstOrDefaultAsync(b => b.Server.DiscordId == serverId.ToString() &&
                                          b.User.DiscordId == userId.ToString());
        }

        public Task<List<Birthday>> GetBirthdaysByDateAsync()
        {
            return context.Birthdays
                .Include(b => b.User)
                .Include(b => b.Server)
                .Where(b => b.Date.Month == DateTime.UtcNow.Month && b.Date.Day == DateTime.UtcNow.Day)
                .ToListAsync();
        }

        public Task<List<Birthday>> GetBirthdaysByServerAsync(ulong serverId)
        {
            return context.Birthdays
                .Include(b => b.User)
                .Include(b => b.Server)
                .Where(b => b.Server.DiscordId == serverId.ToString())
                .OrderBy(b => b.Date.Month)
                .ThenBy(b => b.Date.Day)
                .ThenBy(b => b.Date.Year)
                .ToListAsync();
        }

        public async Task RemoveBirthdayAsync(Birthday birthday)
        {
            context.Birthdays.Remove(birthday);
            await context.SaveChangesAsync();
        }

        public async Task UpdateBirthdayAsync(Birthday birthday)
        {
            context.Birthdays.Update(birthday);
            await context.SaveChangesAsync();
        }
    }
}

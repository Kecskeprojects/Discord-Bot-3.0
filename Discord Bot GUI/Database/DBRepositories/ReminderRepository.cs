using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories;

public class ReminderRepository(MainDbContext context) : GenericRepository<Reminder>(context), IReminderRepository
{
    public Task<Reminder> GetByIndexAsync(string userId, int reminderOrderId)
    {
        return context.Reminders
            .Include(r => r.User)
            .Where(r => r.User.DiscordId == userId.ToString())
            .OrderBy(r => r.Date)
            .Skip(reminderOrderId - 1)
            .FirstAsync();
    }
}

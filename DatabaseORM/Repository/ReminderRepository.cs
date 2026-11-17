using DatabaseORM.Context;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Model;
using Microsoft.EntityFrameworkCore;

namespace DatabaseORM.Repository;

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

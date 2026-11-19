using DatabaseORM.Context;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Model;
using Microsoft.EntityFrameworkCore;

namespace DatabaseORM.Repository;

internal sealed class ReminderRepository(MainDbContext context) : GenericRepository<Reminder>(context), IReminderRepository
{
    public Task<Reminder?> GetByIndexAsync(string userId, int reminderOrderId)
    {
        //SQL Server rejects a negative OFFSET
        if (reminderOrderId < 1)
        {
            return Task.FromResult<Reminder?>(null);
        }

        return context.Reminders
            .Include(r => r.User)
            .Where(r => r.User.DiscordId == userId.ToString())
            .OrderBy(r => r.Date)
            .Skip(reminderOrderId - 1)
            .FirstOrDefaultAsync();
    }
}

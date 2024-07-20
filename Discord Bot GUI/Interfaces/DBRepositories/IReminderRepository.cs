using Discord_Bot.Database.Models;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBRepositories;
public interface IReminderRepository : IGenericRepository<Reminder>
{
    Task<Reminder> GetByIndexAsync(string userId, int reminderOrderId);
}

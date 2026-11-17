using DatabaseORM.Model;

namespace DatabaseORM.Interface.Repository;

public interface IReminderRepository : IGenericRepository<Reminder>
{
    Task<Reminder> GetByIndexAsync(string userId, int reminderOrderId);
}

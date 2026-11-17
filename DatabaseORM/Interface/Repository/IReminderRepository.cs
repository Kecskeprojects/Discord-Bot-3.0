using DatabaseORM.Model;

namespace DatabaseORM.Interface.Repository;

internal interface IReminderRepository : IGenericRepository<Reminder>
{
    Task<Reminder> GetByIndexAsync(string userId, int reminderOrderId);
}

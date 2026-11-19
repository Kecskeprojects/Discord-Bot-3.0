using DatabaseORM.Model;

namespace DatabaseORM.Interface.Repository;

internal interface IReminderRepository : IGenericRepository<Reminder>
{
    //The user's reminder at that position (from 1, by date); null when there is none
    Task<Reminder?> GetByIndexAsync(string userId, int reminderOrderId);
}

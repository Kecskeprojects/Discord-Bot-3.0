using DatabaseORM.Enums;
using DatabaseORM.Resources;

namespace DatabaseORM.Interface.Service;

public interface IReminderService
{
    Task<DbProcessResultEnum> AddReminderAsync(ulong userId, DateTime date, string remindMessage);
    Task<List<ReminderResource>> GetCurrentRemindersAsync(DateTime dateTime);
    Task<List<ReminderResource>> GetUserReminderListAsync(ulong userId);
    Task<DbProcessResultEnum> RemoveCurrentRemindersAsync(List<int> reminderIds);
    Task<DbProcessResultEnum> RemoveUserReminderAsync(ulong userId, int reminderId);
}

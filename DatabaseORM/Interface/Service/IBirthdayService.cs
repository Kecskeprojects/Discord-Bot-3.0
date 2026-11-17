using DatabaseORM.Enums;
using DatabaseORM.Resources;

namespace DatabaseORM.Interface.Service;

public interface IBirthdayService
{
    Task<DbProcessResultEnum> AddBirthdayAsync(ulong serverId, ulong userId, DateTime date);
    Task<List<BirthdayResource>> GetBirthdaysByDateAsync();
    Task<List<BirthdayResource>> GetServerBirthdayListAsync(ulong serverId);
    Task<DbProcessResultEnum> RemoveBirthdayAsync(ulong serverId, ulong userId);
}

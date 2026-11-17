using DatabaseORM.Enums;
using DatabaseORM.Resources;

namespace DatabaseORM.Interface.Service;

public interface IServerMutedUserService
{
    Task<DbProcessResultEnum> AddMutedUserAsync(ulong serverId, ulong userId, DateTime mutedUntil, string removedRoles);
    Task<List<ServerMutedUserResource>> GetExpiredMutedUsersAsync(DateTime dateTime);
    Task<ServerMutedUserResource?> GetMutedUserByUsernameAsync(ulong serverId, ulong userId);
    Task<DbProcessResultEnum> RemoveMutedUserAsync(ulong serverId, ulong userId);
}

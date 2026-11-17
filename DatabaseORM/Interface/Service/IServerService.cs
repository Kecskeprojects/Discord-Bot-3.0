using DatabaseORM.Enums;
using DatabaseORM.Resources;

namespace DatabaseORM.Interface.Service;

public interface IServerService
{
    Task<DbProcessResultEnum> AddServerAsync(ulong serverId);
    Task<DbProcessResultEnum> ChangeRoleMessageIdAsync(ulong serverId, ulong messageId);
    Task<DbProcessResultEnum> AddNotificationRoleAsync(ulong serverId, ulong roleId, string roleName);
    Task<DbProcessResultEnum> RemoveNotificationRoleAsync(ulong serverId);
    Task<ServerResource?> GetByDiscordIdAsync(ulong serverId);
    Task<DbProcessResultEnum> ChangeServerMuteRoleAsync(ulong serverId, string roleName, ulong roleId);
}

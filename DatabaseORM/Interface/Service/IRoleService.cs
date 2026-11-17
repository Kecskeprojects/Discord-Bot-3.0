using DatabaseORM.Enums;
using DatabaseORM.Resources;

namespace DatabaseORM.Interface.Service;

public interface IRoleService
{
    Task<DbProcessResultEnum> AddSelfRoleAsync(ulong serverId, string roleName, ulong roleId);
    Task<RoleResource?> GetRoleAsync(ulong serverId, string roleName);
    Task<List<RoleResource>> GetServerRolesAsync(ulong id);
    Task<DbProcessResultEnum> RemoveSelfRoleAsync(ulong serverId, string roleName);
}

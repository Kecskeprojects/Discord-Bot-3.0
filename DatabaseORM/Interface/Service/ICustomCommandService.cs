using DatabaseORM.Enums;
using DatabaseORM.Resources;

namespace DatabaseORM.Interface.Service;

public interface ICustomCommandService
{
    Task<DbProcessResultEnum> AddCustomCommandAsync(ulong serverId, string commandName, string link);
    Task<CustomCommandResource?> GetCustomCommandAsync(ulong serverId, string commandName);
    Task<List<CustomCommandResource>> GetServerCustomCommandListAsync(ulong serverId);
    Task<DbProcessResultEnum> RemoveCustomCommandAsync(ulong serverId, string commandName);
}

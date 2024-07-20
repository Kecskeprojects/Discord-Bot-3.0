using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices;
public interface ICustomCommandService
{
    Task<DbProcessResultEnum> AddCustomCommandAsync(ulong serverId, string commandName, string link);
    Task<CustomCommandResource> GetCustomCommandAsync(ulong serverId, string commandName);
    Task<List<CustomCommandResource>> GetServerCustomCommandListAsync(ulong serverId);
    Task<DbProcessResultEnum> RemoveCustomCommandAsync(ulong serverId, string commandName);
}

using Discord_Bot.Database.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBRepositories
{
    public interface ICustomCommandRepository
    {
        Task AddCustomCommandAsync(CustomCommand command);
        Task<bool> CustomCommandExistsAsync(ulong serverId, string commandName);
        Task<CustomCommand> GetCustomCommandAsync(ulong serverId, string commandName);
        Task<List<CustomCommand>> GetCustomCommandListAsync(ulong serverId);
        Task RemoveCustomCommandAsync(CustomCommand customCommand);
    }
}

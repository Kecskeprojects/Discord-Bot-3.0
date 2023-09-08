using Discord_Bot.Resources;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices
{
    public interface ICustomCommandService
    {
        Task<CustomCommandResource> GetCustomCommandAsync(ulong id, string commandName);
        Task<List<CustomCommandResource>> GetServerCustomCommandListAsync(ulong id);
    }
}

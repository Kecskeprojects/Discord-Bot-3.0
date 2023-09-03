using Discord_Bot.Database.Models;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBRepositories
{
    public interface ICustomCommandRepository
    {
        Task<CustomCommand> GetCustomCommandAsync(ulong id, string commandName);
    }
}

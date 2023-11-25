using Discord_Bot.Database.Models;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBRepositories
{
    public interface IUserRepository
    {
        Task<User> GetUserByDiscordId(ulong userId);
        Task<User> GetUserWithIdolsByDiscordIdAsync(ulong userId);
        Task UpdateUserAsync(User user);
    }
}

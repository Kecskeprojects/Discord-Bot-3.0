using Discord_Bot.Database.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBRepositories
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllLastFmUsersAsync();
        Task<User> GetUserByDiscordIdAsync(ulong userId);
        Task<User> GetUserWithIdolsByDiscordIdAsync(ulong userId);
        Task UpdateUserAsync(User user);
    }
}

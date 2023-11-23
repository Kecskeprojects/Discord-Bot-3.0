using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories
{
    public class UserRepository(MainDbContext context) : BaseRepository(context), IUserRepository
    {
        public Task<User> GetUserByDiscordId(ulong userId)
        {
            return context.Users
                .FirstOrDefaultAsync(u => u.DiscordId == userId.ToString());
        }
    }
}

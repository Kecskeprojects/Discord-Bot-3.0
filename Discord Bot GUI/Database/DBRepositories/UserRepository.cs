using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories
{
    public class UserRepository(MainDbContext context) : BaseRepository(context), IUserRepository
    {
        public Task<List<User>> GetAllLastFmUsersAsync()
        {
            return context.Users
                .Where(u => !string.IsNullOrEmpty(u.LastFmusername))
                .ToListAsync();
        }

        public Task<User> GetUserByDiscordIdAsync(ulong userId)
        {
            return context.Users
                .FirstOrDefaultAsync(u => u.DiscordId == userId.ToString());
        }

        public Task<User> GetUserWithIdolsByDiscordIdAsync(ulong userId)
        {
            return context.Users
                .Include(u => u.Idols)
                .FirstOrDefaultAsync(u => u.DiscordId == userId.ToString());
        }

        public async Task UpdateUserAsync(User user)
        {
            context.Users.Update(user);
            await context.SaveChangesAsync();
        }
    }
}

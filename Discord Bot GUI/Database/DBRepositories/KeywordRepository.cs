using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories
{
    public class KeywordRepository(MainDbContext context) : BaseRepository(context), IKeywordRepository
    {
        public async Task AddCustomCommandAsync(Keyword keyword)
        {
            context.Keywords.Add(keyword);
            await context.SaveChangesAsync();
        }

        public Task<Keyword> GetKeywordAsync(ulong serverId, string trigger)
        {
            return context.Keywords
                .Include(kw => kw.Server)
                .FirstOrDefaultAsync(kw => kw.Server.DiscordId == serverId.ToString() && kw.Trigger.Trim().ToLower().Equals(trigger.Trim().ToLower()));
        }

        public Task<bool> KeywordExistsAsync(ulong serverId, string trigger)
        {
            return context.Keywords
                .Include(kw => kw.Server)
                .Where(kw => kw.Server.DiscordId == serverId.ToString() && kw.Trigger.Trim().ToLower().Equals(trigger.Trim().ToLower()))
                .AnyAsync();
        }

        public async Task RemoveKeywordAsync(Keyword keyword)
        {
            context.Keywords.Remove(keyword);
            await context.SaveChangesAsync();
        }
    }
}

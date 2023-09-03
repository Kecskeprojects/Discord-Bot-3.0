using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories
{
    public class KeywordRepository : BaseRepository, IKeywordRepository
    {
        public KeywordRepository(MainDbContext context) : base(context)
        {
        }

        public Task<Keyword> GetRoleAsync(ulong serverId, string trigger)
        {
            return context.Keywords
                .Include(kw => kw.Server)
                .FirstOrDefaultAsync(kw => kw.Server.DiscordId == serverId.ToString() && kw.Trigger.Trim().ToLower() == trigger.Trim().ToLower());
        }
    }
}

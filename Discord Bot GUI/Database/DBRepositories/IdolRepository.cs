using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories
{
    public class IdolRepository(MainDbContext context) : BaseRepository(context), IIdolRepository
    {
        public async Task AddIdolAsync(Idol idol)
        {
            context.Idols.Add(idol);
            await context.SaveChangesAsync();
        }

        public Task<List<Idol>> GetIdolsByGroupAsync(string groupName)
        {
            return context.Idols
                .Include(i => i.Group)
                .Where(i => string.IsNullOrEmpty(groupName) || i.Group.Name == groupName)
                .ToListAsync();
        }
        public Task<List<Idol>> GetIdolsByNamesAsync(string[] nameList)
        {
            return context.Idols
                .Include(i => i.Users)
                .Include(i => i.IdolAliases)
                .Include(i => i.Group)
                .Where(i => nameList.Contains(i.Name) || nameList.Contains(i.Group.Name) || nameList.Intersect(i.IdolAliases.Select(ia => ia.Alias)).Any())
                .ToListAsync();
        }

        public Task<List<Idol>> GetIdolsByNameAndGroupAsync(string idolName, string idolGroup)
        {
            return context.Idols
                .Include(i => i.Users)
                .Include(i => i.IdolAliases)
                .Include(i => i.Group)
                .Where(i => (string.IsNullOrEmpty(idolGroup) || idolGroup == i.Group.Name) && (i.Name == idolName || i.IdolAliases.FirstOrDefault(ia => ia.Alias == idolName) != null))
                .ToListAsync();
        }

        public Task<List<Idol>> GetUserIdolsListAsync(ulong userId, string groupName)
        {
            return context.Idols
                .Include(i => i.Users)
                .Include(i => i.Group)
                .Where(i => (string.IsNullOrEmpty(groupName) || i.Group.Name == groupName) && i.Users.FirstOrDefault(u => u.DiscordId == userId.ToString()) != null)
                .ToListAsync();
        }

        public Task<bool> IdolExistsAsync(string idolName, string idolGroup)
        {
            return context.Idols
                .Include(i => i.Group)
                .Where(i => i.Name == idolName && i.Group.Name == idolGroup)
                .AnyAsync();

        }

        public async Task RemoveIdolAsync(Idol idol)
        {
            context.Idols.Remove(idol);
            await context.SaveChangesAsync();
        }

        public Task<Idol> GetIdolByNameAndGroupAsync(string idolName, string idolGroup)
        {
            return context.Idols
                .Include(i => i.Group)
                .FirstOrDefaultAsync(i => i.Name == idolName && i.Group.Name == idolGroup);
        }

        public Task<Idol> GetIdolWithAliasesAsync(string idolName, string idolGroup)
        {
            return context.Idols
                .Include(i => i.IdolAliases)
                .Include(i => i.Group)
                .FirstOrDefaultAsync(i => idolGroup == i.Group.Name && i.Name == idolName);
        }

        public async Task UpdateIdolAsync(Idol idol)
        {
            context.Idols.Update(idol);
            await context.SaveChangesAsync();
        }
    }
}

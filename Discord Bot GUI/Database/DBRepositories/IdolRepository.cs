using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Gaming.Preview.GamesEnumeration;

namespace Discord_Bot.Database.DBRepositories
{
    public class IdolRepository(MainDbContext context) : BaseRepository(context), IIdolRepository
    {
        public Task AddCustomCommandAsync(Idol idol)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<Idol>> GetBiasesByGroupAsync(string groupName)
        {
            return context.Idols
                .Include(i => i.Group)
                .Where(i => i.Group.Name == groupName)
                .ToListAsync();
        }
        public Task<List<Idol>> GetBiasesByNamesAsync(string[] nameList)
        {
            return context.Idols
                .Include(i => i.Users)
                .Include(i => i.IdolAliases)
                .Include(i => i.Group)
                .Where(i => nameList.Contains(i.Name) || nameList.Contains(i.Group.Name) || nameList.Intersect(i.IdolAliases.Select(ia => ia.Alias)).Any())
                .ToListAsync();
        }

        public Task<List<Idol>> GetBiasesByNameAndGroupAsync(string biasName, string biasGroup)
        {
            return context.Idols
                .Include(i => i.Users)
                .Include(i => i.IdolAliases)
                .Include(i => i.Group)
                .Where(i => (string.IsNullOrEmpty(biasGroup) || biasGroup == i.Group.Name) && (i.Name == biasName || i.IdolAliases.FirstOrDefault(ia => ia.Alias == biasName) != null))
                .ToListAsync();
        }

        public Task<List<Idol>> GetUserBiasesListAsync(ulong userId, string groupName)
        {
            return context.Idols
                .Include(i => i.Users)
                .Include(i => i.Group)
                .Where(i => (string.IsNullOrEmpty(groupName) || i.Group.Name == groupName) && i.Users.FirstOrDefault(u => u.DiscordId == userId.ToString()) != null)
                .ToListAsync();
        }

        public Task<bool> IdolExistsAsync(string biasName, string biasGroup)
        {
            return context.Idols
                .Include(i => i.Group)
                .Where(i => i.Name == biasName && i.Group.Name == biasGroup)
                .AnyAsync();

        }

        public async Task RemoveIdolAsync(Idol idol)
        {
            context.Idols.Remove(idol);
            await context.SaveChangesAsync();
        }

        public Task<Idol> GetBiasByNameAndGroupAsync(string biasName, string biasGroup)
        {
            return context.Idols
                .Include(i => i.Group)
                .FirstOrDefaultAsync(i => i.Name == biasName && i.Group.Name == biasGroup);
        }
    }
}

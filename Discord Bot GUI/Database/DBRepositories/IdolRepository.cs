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
        public Task<List<Idol>> GetBiasesByNamesAsync(string[] nameList)
        {
            return context.Idols
                .Include(i => i.Users)
                .Include(i => i.IdolAliases)
                .Include(i => i.Group)
                .Where(i => nameList.Contains(i.Name) || nameList.Contains(i.Group.Name) || nameList.Intersect(i.IdolAliases.Select(ia => ia.Alias)).Any())
                .ToListAsync();
        }
    }
}

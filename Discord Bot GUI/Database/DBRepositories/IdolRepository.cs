using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories
{
    public class IdolRepository(MainDbContext context) : GenericRepository<Idol>(context), IIdolRepository
    {
        public Task<List<Idol>> GetListByNamesAsync(string idolOrGroupName, string idolGroup, string userId = null)
        {
            IQueryable<Idol> dbSet = context.Idols
                .Include(i => i.Users)
                .Include(i => i.IdolAliases)
                .Include(i => i.Group);

            dbSet = dbSet.Where(i =>
                string.IsNullOrEmpty(userId) ||
                i.Users.FirstOrDefault(u => u.DiscordId == userId.ToString()) != null);

            dbSet = dbSet.Where(i =>
                string.IsNullOrEmpty(idolGroup) ||
                idolGroup == i.Group.Name);

            return dbSet.Where(i =>
                i.Name == idolOrGroupName ||
                i.IdolAliases.FirstOrDefault(ia => ia.Alias == idolOrGroupName) != null ||
                i.Group.Name == idolOrGroupName)
                .ToListAsync();
        }
    }
}

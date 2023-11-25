using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories
{
    public class IdolAliasRepository(MainDbContext context) : BaseRepository(context), IIdolAliasRepository
    {
        public Task<IdolAlias> GetIdolAliasAsync(string idolAlias, string idolName, string idolGroup)
        {
            return context.IdolAliases
                .Include(ia => ia.Idol)
                .ThenInclude(i => i.Group)
                .FirstOrDefaultAsync(ia => ia.Alias == idolAlias && ia.Idol.Name == idolName && ia.Idol.Group.Name == idolGroup);
        }

        public Task<bool> IdolAliasExistsAsync(string idolAlias, string idolName, string idolGroup)
        {
            return context.IdolAliases
                .Include(ia => ia.Idol)
                .ThenInclude(i => i.Group)
                .Where(ia => ia.Alias == idolAlias && ia.Idol.Name == idolName && ia.Idol.Group.Name == idolGroup)
                .AnyAsync();
        }

        public async Task RemoveIdolAliasAsync(IdolAlias idolAliasItem)
        {
            context.IdolAliases.Add(idolAliasItem);
            await context.SaveChangesAsync();
        }
    }
}

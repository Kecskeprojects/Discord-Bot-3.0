using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories;

public class IdolImageRepository(MainDbContext context) : GenericRepository<IdolImage>(context), IIdolImageRepository
{
    public Task<IdolImage> GetLatestByIdolIdAsync(int idolId)
    {
        return context.IdolImages
            .Where(ii => ii.IdolId == idolId)
            .OrderByDescending(ii => ii.CreatedOn)
            .FirstOrDefaultAsync();
    }
}

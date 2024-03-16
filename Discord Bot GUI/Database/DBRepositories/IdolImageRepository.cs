using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories
{
    public class IdolImageRepository(MainDbContext context) : BaseRepository(context), IIdolImageRepository
    {
        public async Task RemoveRangeAsync(ICollection<IdolImage> idolImages)
        {
            context.IdolImages.RemoveRange(idolImages);
            await context.SaveChangesAsync();
        }
    }
}

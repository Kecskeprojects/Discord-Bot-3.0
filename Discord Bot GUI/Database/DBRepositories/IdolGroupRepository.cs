using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories
{
    public class IdolGroupRepository(MainDbContext context) : BaseRepository(context), IIdolGroupRepository
    {
        public Task<IdolGroup> GetGroupAsync(string biasGroup)
        {
            return context.IdolGroups
                .FirstOrDefaultAsync(ig => ig.Name == biasGroup);
        }
    }
}

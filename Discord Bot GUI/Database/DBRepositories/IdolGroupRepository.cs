using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories
{
    public class IdolGroupRepository(MainDbContext context) : BaseRepository(context), IIdolGroupRepository
    {
        public async Task AddGroupAsync(IdolGroup newGroup)
        {
            await context.IdolGroups.AddAsync(newGroup);
        }

        public Task<IdolGroup> FirstOrDefaultByIdAsync(int groupId)
        {
            return context.IdolGroups
                .FirstOrDefaultAsync(ig => ig.GroupId ==  groupId);
        }

        public Task<IdolGroup> GetGroupAsync(string idolGroup)
        {
            return context.IdolGroups
                .FirstOrDefaultAsync(ig => ig.Name == idolGroup);
        }

        public Task SaveChangesAsync()
        {
            return context.SaveChangesAsync();
        }
    }
}

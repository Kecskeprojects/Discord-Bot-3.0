using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories
{
    public class IdolGroupRepository(MainDbContext context) : BaseRepository(context), IIdolGroupRepository
    {
        public Task<IdolGroup> GetGroupAsync(string biasGroup)
        {
            throw new System.NotImplementedException();
        }
    }
}

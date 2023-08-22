using Discord_Bot.Core.Logger;

namespace Discord_Bot.Database.DBRepositories
{
    public class BaseRepository
    {
        protected readonly MainDbContext context;

        public BaseRepository(MainDbContext context)
        {
            this.context = context;
        }
    }
}

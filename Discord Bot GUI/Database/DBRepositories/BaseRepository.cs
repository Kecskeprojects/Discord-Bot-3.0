using Discord_Bot.Core.Logger;

namespace Discord_Bot.Database.DBRepositories
{
    public class BaseRepository
    {
        protected readonly MainDbContext context;
        protected readonly Logging logger;

        public BaseRepository(MainDbContext context, Logging logger)
        {
            this.context = context;
            this.logger = logger;
        }
    }
}

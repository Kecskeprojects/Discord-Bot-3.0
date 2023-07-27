using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Logger;

namespace Discord_Bot.Database.DBRepositories
{
    public class ServerRepository : BaseRepository, IServerRepository
    {
        public ServerRepository(MainDbContext context, Logging logger) : base(context, logger)
        {
        }
    }
}

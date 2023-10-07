using Discord_Bot.Interfaces.DBRepositories;

namespace Discord_Bot.Database.DBRepositories
{
    public class ChannelRepository : BaseRepository, IChannelRepository
    {
        public ChannelRepository(MainDbContext context) : base(context)
        {
        }
    }
}

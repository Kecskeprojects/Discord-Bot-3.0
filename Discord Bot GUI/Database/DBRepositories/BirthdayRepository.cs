using Discord_Bot.Interfaces.DBRepositories;

namespace Discord_Bot.Database.DBRepositories
{
    public class BirthdayRepository : BaseRepository, IBirthdayRepository
    {
        public BirthdayRepository(MainDbContext context) : base(context)
        {
        }
    }
}

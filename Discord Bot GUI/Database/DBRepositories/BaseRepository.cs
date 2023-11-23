namespace Discord_Bot.Database.DBRepositories
{
    public class BaseRepository(MainDbContext context)
    {
        protected readonly MainDbContext context = context;
    }
}

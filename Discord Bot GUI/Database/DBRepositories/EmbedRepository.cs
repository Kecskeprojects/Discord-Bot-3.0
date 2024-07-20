using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;

namespace Discord_Bot.Database.DBRepositories;
public class EmbedRepository(MainDbContext context) : GenericRepository<Embed>(context), IEmbedRepository
{
}

using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;

namespace Discord_Bot.Database.DBRepositories;

public class WeeklyPollOptionRepository(MainDbContext context) : GenericRepository<WeeklyPollOption>(context), IWeeklyPollOptionRepository
{
}

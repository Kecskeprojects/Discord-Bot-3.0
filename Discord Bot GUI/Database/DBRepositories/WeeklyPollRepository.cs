using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;

namespace Discord_Bot.Database.DBRepositories;
public class WeeklyPollRepository(MainDbContext context) : GenericRepository<WeeklyPoll>(context), IWeeklyPollRepository
{
}

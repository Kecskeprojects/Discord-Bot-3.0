using DatabaseORM.Context;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Model;

namespace DatabaseORM.Repository;

internal sealed class WeeklyPollRepository(MainDbContext context) : GenericRepository<WeeklyPoll>(context), IWeeklyPollRepository
{
}

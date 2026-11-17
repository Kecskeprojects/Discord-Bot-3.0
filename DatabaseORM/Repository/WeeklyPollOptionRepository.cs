using DatabaseORM.Context;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Model;

namespace DatabaseORM.Repository;

internal sealed class WeeklyPollOptionRepository(MainDbContext context) : GenericRepository<WeeklyPollOption>(context), IWeeklyPollOptionRepository
{
}

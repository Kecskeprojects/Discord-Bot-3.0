using DatabaseORM.Context;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Model;

namespace DatabaseORM.Repository;

public class WeeklyPollRepository(MainDbContext context) : GenericRepository<WeeklyPoll>(context), IWeeklyPollRepository
{
}

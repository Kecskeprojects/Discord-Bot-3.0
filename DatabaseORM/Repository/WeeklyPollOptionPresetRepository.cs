using DatabaseORM.Context;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Model;

namespace DatabaseORM.Repository;

public class WeeklyPollOptionPresetRepository(MainDbContext context) : GenericRepository<WeeklyPollOptionPreset>(context), IWeeklyPollOptionPresetRepository
{
}

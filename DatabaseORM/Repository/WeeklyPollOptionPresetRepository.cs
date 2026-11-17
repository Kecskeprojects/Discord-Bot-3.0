using DatabaseORM.Context;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Model;

namespace DatabaseORM.Repository;

internal sealed class WeeklyPollOptionPresetRepository(MainDbContext context) : GenericRepository<WeeklyPollOptionPreset>(context), IWeeklyPollOptionPresetRepository
{
}

using DatabaseORM.Context;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Model;

namespace DatabaseORM.Repository;

public class TwitchChannelRepository(MainDbContext context) : GenericRepository<TwitchChannel>(context), ITwitchChannelRepository
{
}

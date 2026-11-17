using DatabaseORM.Context;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Model;

namespace DatabaseORM.Repository;

internal sealed class ChannelTypeRepository(MainDbContext context) : GenericRepository<ChannelType>(context), IChannelTypeRepository
{
}

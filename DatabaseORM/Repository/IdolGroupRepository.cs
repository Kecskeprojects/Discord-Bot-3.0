using DatabaseORM.Context;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Model;

namespace DatabaseORM.Repository;

internal sealed class IdolGroupRepository(MainDbContext context) : GenericRepository<IdolGroup>(context), IIdolGroupRepository
{
}

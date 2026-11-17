using DatabaseORM.Context;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Model;

namespace DatabaseORM.Repository;

internal sealed class IdolAliasRepository(MainDbContext context) : GenericRepository<IdolAlias>(context), IIdolAliasRepository
{
}

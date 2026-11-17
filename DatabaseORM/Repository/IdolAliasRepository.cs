using DatabaseORM.Context;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Model;

namespace DatabaseORM.Repository;

public class IdolAliasRepository(MainDbContext context) : GenericRepository<IdolAlias>(context), IIdolAliasRepository
{
}

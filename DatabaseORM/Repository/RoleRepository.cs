using DatabaseORM.Context;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Model;

namespace DatabaseORM.Repository;

internal sealed class RoleRepository(MainDbContext context) : GenericRepository<Role>(context), IRoleRepository
{
}

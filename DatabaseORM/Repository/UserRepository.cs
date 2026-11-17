using DatabaseORM.Context;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Model;

namespace DatabaseORM.Repository;

internal sealed class UserRepository(MainDbContext context) : GenericRepository<User>(context), IUserRepository
{
}

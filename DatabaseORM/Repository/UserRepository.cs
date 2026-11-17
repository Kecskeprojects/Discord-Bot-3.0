using DatabaseORM.Context;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Model;

namespace DatabaseORM.Repository;

public class UserRepository(MainDbContext context) : GenericRepository<User>(context), IUserRepository
{
}

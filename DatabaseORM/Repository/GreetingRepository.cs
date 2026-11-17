using DatabaseORM.Context;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Model;

namespace DatabaseORM.Repository;

internal sealed class GreetingRepository(MainDbContext context) : GenericRepository<Greeting>(context), IGreetingRepository
{
}

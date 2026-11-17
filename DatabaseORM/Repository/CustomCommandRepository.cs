using DatabaseORM.Context;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Model;

namespace DatabaseORM.Repository;

internal sealed class CustomCommandRepository(MainDbContext context) : GenericRepository<CustomCommand>(context), ICustomCommandRepository
{
}

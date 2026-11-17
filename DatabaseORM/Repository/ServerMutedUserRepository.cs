using DatabaseORM.Context;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Model;

namespace DatabaseORM.Repository;

internal sealed class ServerMutedUserRepository(MainDbContext context) : GenericRepository<ServerMutedUser>(context), IServerMutedUserRepository
{
}

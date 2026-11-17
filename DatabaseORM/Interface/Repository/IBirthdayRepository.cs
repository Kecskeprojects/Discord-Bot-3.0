using DatabaseORM.Model;

namespace DatabaseORM.Interface.Repository;

internal interface IBirthdayRepository : IGenericRepository<Birthday>
{
    Task<List<Birthday>> GetListForServerAsync(string serverId);
}

using DatabaseORM.Model;

namespace DatabaseORM.Interface.Repository;

public interface IBirthdayRepository : IGenericRepository<Birthday>
{
    Task<List<Birthday>> GetListForServerAsync(string serverId);
}

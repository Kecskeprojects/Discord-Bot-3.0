using DatabaseORM.Enums;
using DatabaseORM.Model;
using DatabaseORM.Resources;

namespace DatabaseORM.Interface.Repository;

public interface IUserIdolStatisticRepository : IGenericRepository<UserIdolStatistic>
{
    Task<List<UserIdolStatisticResource>> GetTop10ForUserAsync(int userId, GenderEnum gender);
}

using DatabaseORM.Enums;
using DatabaseORM.Model;

namespace DatabaseORM.Interface.Repository;

internal interface IIdolRepository : IGenericRepository<Idol>
{
    Task<List<Idol>> GetListByNamesAsync(string idolOrGroupName, string idolGroup, string? userId = null);
    Task<List<Idol>> GetListForGameAsync(GenderEnum gender, int debutAfter, int debutBefore);
}

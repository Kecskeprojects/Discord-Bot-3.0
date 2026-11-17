using DatabaseORM.Model;

namespace DatabaseORM.Interface.Repository;

internal interface IIdolImageRepository : IGenericRepository<IdolImage>
{
    Task<IdolImage?> GetLatestByIdolIdAsync(int idolId);
}

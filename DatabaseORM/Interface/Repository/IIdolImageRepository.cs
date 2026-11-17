using DatabaseORM.Model;

namespace DatabaseORM.Interface.Repository;

public interface IIdolImageRepository : IGenericRepository<IdolImage>
{
    Task<IdolImage?> GetLatestByIdolIdAsync(int idolId);
}

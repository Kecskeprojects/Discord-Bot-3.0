using DatabaseORM.Context;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Model;
using Microsoft.EntityFrameworkCore;

namespace DatabaseORM.Repository;

public class IdolImageRepository(MainDbContext context) : GenericRepository<IdolImage>(context), IIdolImageRepository
{
    public Task<IdolImage?> GetLatestByIdolIdAsync(int idolId)
    {
        return context.IdolImages
            .Where(ii => ii.IdolId == idolId)
            .OrderByDescending(ii => ii.CreatedOn)
            .FirstOrDefaultAsync();
    }
}

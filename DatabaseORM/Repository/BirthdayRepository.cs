using DatabaseORM.Context;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Model;
using Microsoft.EntityFrameworkCore;

namespace DatabaseORM.Repository;

internal sealed class BirthdayRepository(MainDbContext context) : GenericRepository<Birthday>(context), IBirthdayRepository
{
    public Task<List<Birthday>> GetListForServerAsync(string serverId)
    {
        return context.Birthdays
            .Include(b => b.User)
            .Include(b => b.Server)
            .Where(b => b.Server.DiscordId == serverId)
            .OrderBy(b => b.Date.Month)
            .ThenBy(b => b.Date.Day)
            .ThenBy(b => b.Date.Year)
            .ToListAsync();
    }
}

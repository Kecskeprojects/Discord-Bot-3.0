using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories;

public class BirthdayRepository(MainDbContext context) : GenericRepository<Birthday>(context), IBirthdayRepository
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

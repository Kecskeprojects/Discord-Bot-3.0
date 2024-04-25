using Discord_Bot.Database.Models;
using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBRepositories
{
    public interface IUserIdolStatisticRepository : IGenericRepository<UserIdolStatistic>
    {
        Task<List<UserIdolStatisticResource>> GetTop10ForUserAsync(int userId, GenderType gender);
    }
}

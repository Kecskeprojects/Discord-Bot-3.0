using Discord_Bot.Database.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBRepositories
{
    public interface IBirthdayRepository
    {
        Task AddBirthdayAsync(Birthday birthday);
        Task<Birthday> GetBirthdayAsync(ulong serverId, ulong userId);
        Task<List<Birthday>> GetBirthdaysByDateAsync();
        Task RemoveBirthdayAsync(Birthday birthday);
        Task UpdateBirthdayAsync(Birthday birthday);
    }
}

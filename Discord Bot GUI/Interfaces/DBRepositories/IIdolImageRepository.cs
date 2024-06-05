using Discord_Bot.Database.Models;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBRepositories;

public interface IIdolImageRepository : IGenericRepository<IdolImage>
{
    Task<IdolImage> GetLatestByIdolIdAsync(int idolId);
}

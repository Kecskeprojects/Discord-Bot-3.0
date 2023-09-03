using Discord_Bot.Database.Models;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBRepositories
{
    public interface IRoleRepository
    {
        Task<Role> GetRoleAsync(ulong id, string roleName);
    }
}

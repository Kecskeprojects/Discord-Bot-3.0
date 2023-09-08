using Discord_Bot.Resources;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices
{
    public interface IRoleService
    {
        Task<RoleResource> GetRoleAsync(ulong serverId, string roleName);
    }
}

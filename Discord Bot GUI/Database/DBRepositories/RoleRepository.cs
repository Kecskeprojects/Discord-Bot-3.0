using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories
{
    public class RoleRepository : BaseRepository, IRoleRepository
    {
        public RoleRepository(MainDbContext context) : base(context)
        {
        }

        public Task<Role> GetRoleAsync(ulong serverId, string roleName)
        {
            return context.Roles
                .Include(r => r.Server)
                .FirstOrDefaultAsync(r => r.Server.DiscordId == serverId.ToString() && r.RoleName.Trim().ToLower() == roleName.Trim().ToLower());
        }
    }
}

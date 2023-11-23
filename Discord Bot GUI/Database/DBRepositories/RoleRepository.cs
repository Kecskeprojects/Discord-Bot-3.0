using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories
{
    public class RoleRepository(MainDbContext context) : BaseRepository(context), IRoleRepository
    {
        public async Task AddSelfRoleAsync(Role role)
        {
            context.Roles.Add(role);
            await context.SaveChangesAsync();
        }

        public Task<Role> GetRoleAsync(ulong serverId, string roleName)
        {
            return context.Roles
                .Include(r => r.Server)
                .FirstOrDefaultAsync(r => r.Server.DiscordId == serverId.ToString() && r.RoleName.Trim().Equals(roleName.Trim(), System.StringComparison.OrdinalIgnoreCase));
        }

        public async Task RemoveSelfRoleAsync(Role role)
        {
            context.Roles.Remove(role);
            await context.SaveChangesAsync();
        }

        public Task<bool> RoleExistsExistsAsync(ulong serverId, ulong roleId)
        {
            return context.Roles
                .Include(r => r.Server)
                .Where(r => r.Server.DiscordId == serverId.ToString() && r.DiscordId == roleId.ToString())
                .AnyAsync();
        }
    }
}

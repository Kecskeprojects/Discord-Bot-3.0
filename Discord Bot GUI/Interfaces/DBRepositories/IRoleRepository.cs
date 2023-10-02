﻿using Discord_Bot.Database.Models;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBRepositories
{
    public interface IRoleRepository
    {
        Task AddSelfRoleAsync(Role role);
        Task<Role> GetRoleAsync(ulong serverId, string roleName);
        Task RemoveSelfRoleAsync(Role role);
        Task<bool> RoleExistsExistsAsync(ulong serverId, ulong roleId);
    }
}

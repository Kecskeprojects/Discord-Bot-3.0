﻿using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices;

public interface IRoleService
{
    Task<DbProcessResultEnum> AddSelfRoleAsync(ulong serverId, string roleName, ulong roleId);
    Task<RoleResource> GetRoleAsync(ulong serverId, string roleName);
    Task<List<RoleResource>> GetServerRolesAsync(ulong id);
    Task<DbProcessResultEnum> RemoveSelfRoleAsync(ulong serverId, string roleName);
}

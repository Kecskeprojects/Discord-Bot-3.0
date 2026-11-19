using DatabaseORM.Caching;
using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Interface.Service;
using DatabaseORM.Model;
using DatabaseORM.Resources;
using MapsterMapper;

namespace DatabaseORM.Service;

internal sealed class RoleService(
    IRoleRepository roleRepository,
    IServerRepository serverRepository,
    IMapper mapper,
    IBotLogger logger,
    ServerCache cache) : BaseService(mapper, logger, cache), IRoleService
{
    private readonly IRoleRepository roleRepository = roleRepository;
    private readonly IServerRepository serverRepository = serverRepository;

    public async Task<DbProcessResultEnum> AddSelfRoleAsync(ulong serverId, string roleName, ulong roleId)
    {
        try
        {
            if (await roleRepository.ExistsAsync(
                r => r.Server.DiscordId == serverId.ToString()
                && r.DiscordId == roleId.ToString(),
                r => r.Server))
            {
                return DbProcessResultEnum.AlreadyExists;
            }

            Server? server = await serverRepository.FirstOrDefaultAsync(s => s.DiscordId == serverId.ToString());
            if (server == null)
            {
                return DbProcessResultEnum.NotFound;
            }

            Role role = new()
            {
                RoleId = 0,
                Server = server,
                RoleName = roleName,
                DiscordId = roleId.ToString()

            };
            _ = await roleRepository.AddAsync(role);

            logger.Log("Role added successfully!");
            return DbProcessResultEnum.Success;
        }
        catch (Exception ex)
        {
            logger.Error("RoleService.cs AddSelfRoleAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }

    public async Task<RoleResource?> GetRoleAsync(ulong serverId, string roleName)
    {
        RoleResource? result = null;
        try
        {
            roleName = roleName.Trim().ToLower();
            Role? role = await roleRepository.FirstOrDefaultAsync(r =>
                r.Server.DiscordId == serverId.ToString()
                && r.RoleName.Trim().ToLower().Equals(roleName),
                r => r.Server);
            result = role == null ? null : mapper.Map<Role, RoleResource>(role);
        }
        catch (Exception ex)
        {
            logger.Error("RoleService.cs GetRoleAsync", ex);
        }
        return result;
    }

    public async Task<List<RoleResource>> GetServerRolesAsync(ulong id)
    {
        List<RoleResource> result = [];
        try
        {
            List<Role> role = await roleRepository.GetListAsync(r =>
            r.Server.DiscordId == id.ToString(),
            r => r.Server);
            result = mapper.Map<List<Role>, List<RoleResource>>(role);
        }
        catch (Exception ex)
        {
            logger.Error("RoleService.cs GetServerRolesAsync", ex);
        }
        return result;
    }

    public async Task<DbProcessResultEnum> RemoveSelfRoleAsync(ulong serverId, string roleName)
    {
        try
        {
            roleName = roleName.Trim().ToLower();
            Role? role = await roleRepository.FirstOrDefaultAsync(r =>
                r.Server.DiscordId == serverId.ToString()
                && r.RoleName.Trim().ToLower().Equals(roleName));
            if (role != null)
            {
                _ = await roleRepository.RemoveAsync(role);

                logger.Log($"Role {roleName} removed successfully!");
                return DbProcessResultEnum.Success;
            }
            else
            {
                logger.Log($"Role {roleName} could not be found!");
                return DbProcessResultEnum.NotFound;
            }
        }
        catch (Exception ex)
        {
            logger.Error("RoleService.cs RemoveSelfRoleAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }
}

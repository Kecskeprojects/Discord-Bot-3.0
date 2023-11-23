using AutoMapper;
using Discord_Bot.Core.Caching;
using Discord_Bot.Core.Logger;
using Discord_Bot.Database.Models;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices
{
    public class RoleService(IMapper mapper, Logging logger, Cache cache, IRoleRepository roleRepository, IServerRepository serverRepository) : BaseService(mapper, logger, cache), IRoleService
    {
        private readonly IRoleRepository roleRepository = roleRepository;
        private readonly IServerRepository serverRepository = serverRepository;

        public async Task<DbProcessResultEnum> AddSelfRoleAsync(ulong serverId, string roleName, ulong roleId)
        {
            try
            {
                if (await roleRepository.RoleExistsExistsAsync(serverId, roleId))
                {
                    return DbProcessResultEnum.AlreadyExists;
                }

                Server server = await serverRepository.GetByDiscordIdAsync(serverId);

                Role role = new()
                {
                    RoleId = 0,
                    Server = server,
                    RoleName = roleName,
                    DiscordId = roleId.ToString()

                };
                await roleRepository.AddSelfRoleAsync(role);

                logger.Log("Role added successfully!");
                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("RoleService.cs AddSelfRoleAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<RoleResource> GetRoleAsync(ulong serverId, string roleName)
        {
            RoleResource result = null;
            try
            {
                Role role = await roleRepository.GetRoleAsync(serverId, roleName);
                result = mapper.Map<Role, RoleResource>(role);
            }
            catch (Exception ex)
            {
                logger.Error("RoleService.cs GetRoleAsync", ex.ToString());
            }
            return result;
        }

        public async Task<DbProcessResultEnum> RemoveSelfRoleAsync(ulong serverId, string roleName)
        {
            try
            {
                Role role = await roleRepository.GetRoleAsync(serverId, roleName);
                if (role != null)
                {
                    await roleRepository.RemoveSelfRoleAsync(role);

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
                logger.Error("RoleService.cs RemoveSelfRoleAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }
    }
}

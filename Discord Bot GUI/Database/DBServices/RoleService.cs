using AutoMapper;
using Discord_Bot.Core.Caching;
using Discord_Bot.Core.Logger;
using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices
{
    public class RoleService : BaseService, IRoleService
    {
        private readonly IRoleRepository roleRepository;
        public RoleService(IMapper mapper, Logging logger, Cache cache, IRoleRepository roleRepository) : base(mapper, logger, cache) => this.roleRepository = roleRepository;

        public async Task<RoleResource> GetRoleAsync(ulong id, string roleName)
        {
            RoleResource result = null;
            try
            {
                Role role = await roleRepository.GetRoleAsync(id, roleName);
                result = mapper.Map<Role, RoleResource>(role);
            }
            catch (Exception ex)
            {
                logger.Error("RoleService.cs GetRoleAsync", ex.ToString());
            }
            return result;
        }
    }
}

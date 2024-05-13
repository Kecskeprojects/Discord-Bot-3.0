using AutoMapper;
using Discord_Bot.Core;
using Discord_Bot.Core.Caching;
using Discord_Bot.Database.Models;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices
{
    public class ServerService(
        IServerChannelViewService serverChannelViewService,
        IServerRepository serverRepository,
        IRoleRepository roleRepository,
        IMapper mapper,
        Logging logger,
        Cache cache) : BaseService(mapper, logger, cache), IServerService
    {
        private readonly IServerChannelViewService serverChannelViewService = serverChannelViewService;
        private readonly IServerRepository serverRepository = serverRepository;
        private readonly IRoleRepository roleRepository = roleRepository;

        public async Task<DbProcessResultEnum> AddServerAsync(ulong serverId)
        {
            try
            {
                Server server = new()
                {
                    DiscordId = serverId.ToString()
                };
                await serverRepository.AddAsync(server);

                logger.Log($"Server added with the following ID: {serverId}");
                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("ServerService.cs AddServerAsync", ex);
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<DbProcessResultEnum> ChangeRoleMessageIdAsync(ulong serverId, ulong messageId)
        {
            try
            {
                Server server = await serverRepository.FirstOrDefaultAsync(s => s.DiscordId == serverId.ToString());

                server.RoleMessageDiscordId = messageId.ToString();

                await serverRepository.SaveChangesAsync();

                cache.RemoveCachedEntityManually(serverId);

                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("ServerService.cs ChangeRoleMessageIdAsync", ex);
            }

            return DbProcessResultEnum.Failure;
        }

        public async Task<DbProcessResultEnum> AddNotificationRoleAsync(ulong serverId, ulong roleId, string roleName)
        {
            try
            {
                Server server = await serverRepository.FirstOrDefaultAsync(s => s.DiscordId == serverId.ToString());
                if (server == null)
                {
                    return DbProcessResultEnum.NotFound;
                }

                roleName = roleName.Trim().ToLower();
                Role role = await roleRepository.FirstOrDefaultAsync( r =>
                    r.Server.DiscordId == serverId.ToString()
                    && r.RoleName.Trim().ToLower().Equals(roleName),
                    r => r.Server);
                role ??= new()
                {
                    RoleId = 0,
                    DiscordId = roleId.ToString(),
                    RoleName = roleName,
                    Server = server,
                    Servers = []
                };

                if (role.RoleId == server.NotificationRoleId)
                {
                    return DbProcessResultEnum.AlreadyExists;
                }

                server.NotificationRole = role;

                await serverRepository.UpdateAsync(server);

                cache.RemoveCachedEntityManually(serverId);

                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("ServerService.cs AddNotificationRoleAsync", ex);
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<DbProcessResultEnum> RemoveNotificationRoleAsync(ulong serverId)
        {
            try
            {
                Server server = await serverRepository.FirstOrDefaultAsync(s => s.DiscordId == serverId.ToString());
                if (server == null || server.NotificationRoleId == null)
                {
                    return DbProcessResultEnum.NotFound;
                }

                server.NotificationRole = null;

                await serverRepository.SaveChangesAsync();

                cache.RemoveCachedEntityManually(serverId);

                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("ServerService.cs RemoveNotificationRoleAsync", ex);
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<ServerResource> GetByDiscordIdAsync(ulong serverId)
        {
            ServerResource result = null;
            try
            {
                ServerResource cachedServer = cache.TryGetValue(serverId);
                if (cachedServer != null)
                {
                    return cachedServer;
                }

                Server server = await serverRepository.FirstOrDefaultAsync(s =>
                    s.DiscordId == serverId.ToString(),
                    s => s.TwitchChannels,
                    s => s.NotificationRole);
                if (server == null)
                {
                    return null;
                }

                result = mapper.Map<Server, ServerResource>(server);
                result.SettingsChannels = await serverChannelViewService.GetServerChannelsAsync(result.ServerId);

                cache.TryAddValue(result.DiscordId, result);
            }
            catch (Exception ex)
            {
                logger.Error("ServerService.cs GetByDiscordIdAsync", ex);
            }

            return result;
        }
    }
}

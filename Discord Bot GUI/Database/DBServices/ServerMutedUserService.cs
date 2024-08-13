using AutoMapper;
using Discord_Bot.Core;
using Discord_Bot.Core.Caching;
using Discord_Bot.Database.Models;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices;
public class ServerMutedUserService(
    IServerMutedUserRepository serverMutedUserRepository,
    IUserRepository userRepository,
    IServerRepository serverRepository,
    IMapper mapper,
    BotLogger logger,
    ServerCache cache) : BaseService(mapper, logger, cache), IServerMutedUserService
{
    private readonly IServerMutedUserRepository serverMutedUserRepository = serverMutedUserRepository;
    private readonly IUserRepository userRepository = userRepository;

    public async Task<DbProcessResultEnum> AddMutedUserAsync(ulong serverId, ulong userId, DateTime mutedUntil)
    {
        try
        {
            User user = await userRepository.FirstOrDefaultAsync(u => u.DiscordId == userId.ToString());
            Server server = await serverRepository.FirstOrDefaultAsync(u => u.DiscordId == serverId.ToString());

            if(await serverMutedUserRepository.ExistsAsync(smu => smu.UserId == user.UserId && smu.ServerId == server.ServerId))
            {
                return DbProcessResultEnum.AlreadyExists;
            }

            ServerMutedUser mutedUser = new()
            {
                MutedUntil = mutedUntil,
                User = user ?? new() { DiscordId = userId.ToString() },
                Server = server ?? new() { DiscordId = serverId.ToString() },
            };

            await serverMutedUserRepository.AddAsync(mutedUser);

            logger.Log($"MutedUser added with the following ID: {serverId}");
            return DbProcessResultEnum.Success;
        }
        catch (Exception ex)
        {
            logger.Error("ServerMutedUserService.cs AddMutedUserAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }

    public async Task<DbProcessResultEnum> RemoveMutedUserAsync(ulong serverId, ulong userId)
    {
        try
        {
            User user = await userRepository.FirstOrDefaultAsync(u => u.DiscordId == userId.ToString());
            Server server = await serverRepository.FirstOrDefaultAsync(u => u.DiscordId == serverId.ToString());

            ServerMutedUser mutedUser = await serverMutedUserRepository.FirstOrDefaultAsync(smu => smu.UserId == user.UserId && smu.ServerId == server.ServerId);
            if (mutedUser == null)
            {
                return DbProcessResultEnum.NotFound;
            }

            await serverMutedUserRepository.RemoveAsync(mutedUser);

            logger.Log($"MutedUser removed with the following ID: {serverId}");
            return DbProcessResultEnum.Success;
        }
        catch (Exception ex)
        {
            logger.Error("ServerMutedUserService.cs RemoveMutedUserAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }
}

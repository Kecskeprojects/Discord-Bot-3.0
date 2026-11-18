using DatabaseORM.Caching;
using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Interface.Service;
using DatabaseORM.Model;
using DatabaseORM.Resources;
using MapsterMapper;

namespace DatabaseORM.Service;

internal sealed class UserService(
    IUserRepository userRepository,
    IUserIdolStatisticRepository userIdolStatisticRepository,
    IMapper mapper,
    IBotLogger logger,
    ServerCache cache) : BaseService(mapper, logger, cache), IUserService
{
    private readonly IUserRepository userRepository = userRepository;

    public async Task<DbProcessResultEnum> AddLastfmUsernameAsync(ulong userId, string name)
    {
        try
        {
            User? user = await userRepository.FirstOrDefaultAsync(u => u.DiscordId == userId.ToString());

            user ??= new User()
            {
                DiscordId = userId.ToString()
            };

            if (!string.IsNullOrEmpty(user.LastFmusername))
            {
                return DbProcessResultEnum.AlreadyExists;
            }
            user.LastFmusername = name;
            _ = await userRepository.UpdateAsync(user);

            logger.Log("Lastfm username added successfully!");
            return DbProcessResultEnum.Success;
        }
        catch (Exception ex)
        {
            logger.Error("UserService.cs AddLastfmUsernameAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }

    public async Task<List<UserResource>> GetAllLastFmUsersAsync()
    {
        List<UserResource> result = [];
        try
        {
            List<User> users = await userRepository.GetListAsync(u => !string.IsNullOrEmpty(u.LastFmusername));
            result = mapper.Map<List<User>, List<UserResource>>(users);
        }
        catch (Exception ex)
        {
            logger.Error("UserService.cs GetAllLastFmUsersAsync", ex);
        }
        return result;
    }

    public async Task<UserBiasGameStatResource?> GetTopIdolsAsync(ulong userId, GenderEnum gender)
    {
        UserBiasGameStatResource? result = null;
        try
        {
            User? user = await userRepository.FirstOrDefaultAsync(u => u.DiscordId == userId.ToString());
            if (user == null)
            {
                return null;
            }
            result = mapper.Map<User, UserBiasGameStatResource>(user);

            result.Stats = await userIdolStatisticRepository.GetTop10ForUserAsync(user.UserId, gender);
        }
        catch (Exception ex)
        {
            logger.Error("UserService.cs GetTopIdolsAsync", ex);
        }
        return result;
    }

    public async Task<UserResource?> GetUserAsync(ulong userId)
    {
        UserResource? result = null;
        try
        {
            User? user = await userRepository.FirstOrDefaultAsync(u => u.DiscordId == userId.ToString());
            result = user == null ? null : mapper.Map<User, UserResource>(user);
        }
        catch (Exception ex)
        {
            logger.Error("UserService.cs GetUserAsync", ex);
        }
        return result;
    }

    public async Task<DbProcessResultEnum> RemoveLastfmUsernameAsync(ulong userId)
    {
        try
        {
            User? user = await userRepository.FirstOrDefaultAsync(u => u.DiscordId == userId.ToString());

            if (user == null || string.IsNullOrEmpty(user.LastFmusername))
            {
                return DbProcessResultEnum.NotFound;
            }
            user.LastFmusername = null;
            _ = await userRepository.SaveChangesAsync();

            logger.Log("Lastfm username removed successfully!");
            return DbProcessResultEnum.Success;
        }
        catch (Exception ex)
        {
            logger.Error("UserService.cs RemoveLastfmUsernameAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }
}

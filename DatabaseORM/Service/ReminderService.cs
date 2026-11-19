using DatabaseORM.Caching;
using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Interface.Service;
using DatabaseORM.Model;
using DatabaseORM.Resources;
using MapsterMapper;

namespace DatabaseORM.Service;

internal sealed class ReminderService(
    IReminderRepository reminderRepository,
    IUserRepository userRepository,
    IMapper mapper,
    IBotLogger logger,
    ServerCache cache) : BaseService(mapper, logger, cache), IReminderService
{
    private readonly IReminderRepository reminderRepository = reminderRepository;
    private readonly IUserRepository userRepository = userRepository;

    public async Task<DbProcessResultEnum> AddReminderAsync(ulong userId, DateTime date, string remindMessage)
    {
        try
        {
            User? user = await userRepository.FirstOrDefaultAsync(u => u.DiscordId == userId.ToString());
            Reminder reminder = new()
            {
                ReminderId = 0,
                User = user ?? new User() { DiscordId = userId.ToString() },
                Date = date,
                Message = remindMessage
            };
            _ = await reminderRepository.AddAsync(reminder);

            logger.Log($"Reminder added for the following user: {userId}\nwith the following date: {date:yyyy.MM.dd HH:mm:ss}");
            return DbProcessResultEnum.Success;
        }
        catch (Exception ex)
        {
            logger.Error("ReminderService.cs AddReminderAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }

    public async Task<List<ReminderResource>> GetCurrentRemindersAsync(DateTime dateTime)
    {
        List<ReminderResource> result = [];
        try
        {
            List<Reminder> reminders = await reminderRepository.GetListAsync(r => r.Date <= dateTime, r => r.User);
            if (reminders == null)
            {
                return [];
            }

            result = mapper.Map<List<Reminder>, List<ReminderResource>>(reminders);
        }
        catch (Exception ex)
        {
            logger.Error("ReminderService.cs GetCurrentRemindersAsync", ex);
        }

        return result;
    }

    public async Task<List<ReminderResource>> GetUserReminderListAsync(ulong userId)
    {
        List<ReminderResource> result = [];
        try
        {
            List<Reminder> server = await reminderRepository.GetListAsync(r =>
            r.User.DiscordId == userId.ToString(),
            orderBy: r => r.Date,
            ascending: true,
            includes: r => r.User);

            if (server == null)
            {
                return [];
            }

            result = mapper.Map<List<Reminder>, List<ReminderResource>>(server);
        }
        catch (Exception ex)
        {
            logger.Error("ReminderService.cs GetUserReminderListAsync", ex);
        }

        return result;
    }

    public async Task<DbProcessResultEnum> RemoveCurrentRemindersAsync(List<int> reminderIds)
    {
        try
        {
            List<Reminder> reminders = await reminderRepository.GetListAsync(r => reminderIds.Contains(r.ReminderId));

            if (reminders.Count > 0)
            {
                _ = await reminderRepository.RemoveAsync(reminders);

                logger.Log($"Reminders removed with the following IDs: {string.Join(",", reminderIds)}");
            }
            else
            {
                logger.Log("No reminders to remove currently.");
            }
            return DbProcessResultEnum.Success;
        }
        catch (Exception ex)
        {
            logger.Error("ReminderService.cs RemoveCurrentRemindersAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }

    public async Task<DbProcessResultEnum> RemoveUserReminderAsync(ulong userId, int reminderId)
    {
        try
        {
            Reminder? reminder = await reminderRepository.GetByIndexAsync(userId.ToString(), reminderId);

            if (reminder != null)
            {
                _ = await reminderRepository.RemoveAsync(reminder);

                logger.Log($"Reminders removed by the following user: {userId}\nwith the following ID: {reminderId}");
                return DbProcessResultEnum.Success;
            }
            else
            {
                logger.Log($"Reminder was not found with the following user: {userId}\nand reminder ID: {reminderId}");
                return DbProcessResultEnum.NotFound;
            }
        }
        catch (Exception ex)
        {
            logger.Error("ReminderService.cs RemoveUserReminderAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }
}

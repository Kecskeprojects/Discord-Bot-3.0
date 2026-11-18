using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord_Bot.Tools;
using Discord_Bot.Tools.NativeTools;

namespace Discord_Bot.Features;

public class ReminderFeature(
    IReminderService reminderService,
    IDiscordClient client,
    TimeProvider timeProvider,
    IServerService serverService,
    IBotLogger logger) : BaseFeature(serverService, logger)
{
    private readonly IReminderService reminderService = reminderService;
    private readonly IDiscordClient client = client;
    private readonly TimeProvider timeProvider = timeProvider;

    protected override async Task<bool> ExecuteCoreLogicAsync()
    {
        try
        {
            //Get the list of reminders that are before or exactly set to this minute
            DateTime now = timeProvider.GetUtcNow().UtcDateTime;
            DateTime dateTime = new(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Kind);
            List<ReminderResource> result = await reminderService.GetCurrentRemindersAsync(dateTime);
            if (!CollectionTools.IsNullOrEmpty(result))
            {
                foreach (ReminderResource reminder in result)
                {
                    //Modify message
                    reminder.Message = reminder.Message.Insert(0, $"You told me to remind you at {DiscordTools.UtcTimestamp(reminder.Date, TimestampTagStyles.ShortDateTime)} with the following message:\n\n");

                    //Try getting user
                    IUser user = await client.GetUserAsync(reminder.UserDiscordId);

                    //If user exists send a direct message to the user
                    if (user != null)
                    {
                        _ = await user.SendMessageAsync(reminder.Message);
                    }
                }

                List<int> reminderIds = result.Select(r => r.ReminderId).ToList();
                DbProcessResultEnum reminderResult = await reminderService.RemoveCurrentRemindersAsync(reminderIds);
                if (reminderResult == DbProcessResultEnum.Failure)
                {
                    logger.Error("ReminderFeature.cs ExecuteCoreLogicAsync", "Failure during reminder check!");
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error("ReminderFeature.cs ExecuteCoreLogicAsync", ex);
            return false;
        }
        return true;
    }
}

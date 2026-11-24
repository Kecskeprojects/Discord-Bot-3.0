using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord.Net;
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
            if (CollectionTools.IsNullOrEmpty(result))
            {
                return true;
            }

            //Each reminder on its own, so one that Discord refuses doesn't stop the others
            List<int> handledIds = [];
            foreach (ReminderResource reminder in result)
            {
                if (await TrySendAsync(reminder))
                {
                    handledIds.Add(reminder.ReminderId);
                }
            }

            if (handledIds.Count > 0)
            {
                DbProcessResultEnum reminderResult = await reminderService.RemoveCurrentRemindersAsync(handledIds);
                if (reminderResult == DbProcessResultEnum.Failure)
                {
                    logger.Error("ReminderFeature.cs ExecuteCoreLogicAsync", "Failure during reminder check!");
                    return false;
                }
            }

            return handledIds.Count == result.Count;
        }
        catch (Exception ex)
        {
            logger.Error("ReminderFeature.cs ExecuteCoreLogicAsync", ex);
            return false;
        }
    }

    //False when sending failed: the reminder then stays for the next minute, in case the problem was temporary
    private async Task<bool> TrySendAsync(ReminderResource reminder)
    {
        try
        {
            IUser user = await client.GetUserAsync(reminder.UserDiscordId);

            //A user Discord doesn't know anymore can't be reached later either, so their reminder counts as handled
            if (user != null)
            {
                string message = $"You told me to remind you at {DiscordTools.UtcTimestamp(reminder.Date, TimestampTagStyles.ShortDateTime)} with the following message:\n\n{reminder.Message}";
                _ = await user.SendMessageAsync(message);
            }
            return true;
        }
        //50007: the user closed their direct messages to the bot, or shares no server with it. That rarely changes before
        //the next minute, so the reminder counts as handled; until 2026-11-24 it was retried, with a warning, every minute
        catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
        {
            logger.Warning("ReminderFeature.cs TrySendAsync", $"Discord can't deliver direct messages to user {reminder.UserDiscordId} (closed, or no shared server), so reminder {reminder.ReminderId} is removed unsent");
            return true;
        }
        catch (Exception ex)
        {
            logger.Warning("ReminderFeature.cs TrySendAsync", ex);
            return false;
        }
    }
}

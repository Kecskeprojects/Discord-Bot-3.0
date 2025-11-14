using Discord;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using Discord_Bot.Tools.NativeTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Features;

public class ReminderFeature(
    IReminderService reminderService,
    DiscordSocketClient client,
    IServerService serverService,
    BotLogger logger) : BaseFeature(serverService, logger)
{
    private readonly IReminderService reminderService = reminderService;
    private readonly DiscordSocketClient client = client;

    protected override async Task<bool> ExecuteCoreLogicAsync()
    {
        try
        {
            //Get the list of reminders that are before or exactly set to this minute
            DateTime dateTime = DateTime.Parse(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"));
            List<ReminderResource> result = await reminderService.GetCurrentRemindersAsync(dateTime);
            if (!CollectionTools.IsNullOrEmpty(result))
            {
                foreach (ReminderResource reminder in result)
                {
                    //Modify message
                    reminder.Message = reminder.Message.Insert(0, $"You told me to remind you at `{reminder.Date}` with the following message:\n\n");

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

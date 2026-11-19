using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord_Bot.Processors.MessageProcessor;
using Discord_Bot.Tools.NativeTools;

namespace Discord_Bot.Features;

public class BirthdayFeature(
    IBirthdayService birthdayService,
    IDiscordClient client,
    TimeProvider timeProvider,
    IServerService serverService,
    IBotLogger logger) : BaseFeature(serverService, logger)
{
    private readonly IBirthdayService birthdayService = birthdayService;
    private readonly IDiscordClient client = client;
    private readonly TimeProvider timeProvider = timeProvider;

    protected override async Task<bool> ExecuteCoreLogicAsync()
    {
        try
        {
            //Get the list of birthdays that are on this date
            DateTime today = timeProvider.GetUtcNow().UtcDateTime;
            List<BirthdayResource> result = await birthdayService.GetBirthdaysByDateAsync(today);
            bool allSent = true;
            if (!CollectionTools.IsNullOrEmpty(result))
            {
                logger.Log($"Sending birthday message to {result.Count} people.");
                foreach (BirthdayResource birthday in result)
                {
                    //Each on its own, so a server whose channel refuses the message doesn't stop the greetings of the others
                    try
                    {
                        await SendBirthdayMessageAsync(birthday, today.Year);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("BirthdayFeature.cs SendBirthdayMessageAsync", ex);
                        allSent = false;
                    }
                }
            }
            return allSent;
        }
        catch (Exception ex)
        {
            logger.Error("BirthdayFeature.cs ExecuteCoreLogicAsync", ex);
            return false;
        }
    }

    private async Task SendBirthdayMessageAsync(BirthdayResource birthday, int currentYear)
    {
        ServerResource? server = await serverService.GetByDiscordIdAsync(birthday.ServerDiscordId);
        if (server == null)
        {
            logger.Warning("BirthdayFeature.cs SendBirthdayMessageAsync", $"Server {birthday.ServerDiscordId} could not be loaded, its birthday message is skipped.");
            return;
        }

        if (server.SettingsChannels.TryGetValue(ChannelTypeEnum.BirthdayText, out List<ulong>? channels))
        {
            IGuild? guild = await client.GetGuildAsync(birthday.ServerDiscordId, CacheMode.CacheOnly);

            //A deleted channel or a server the bot left is skipped, so the birthdays of the other servers are still sent
            if (await client.GetChannelAsync(channels[0], CacheMode.CacheOnly) is not IMessageChannel channel || guild == null)
            {
                logger.Warning("BirthdayFeature.cs SendBirthdayMessageAsync", $"Birthday channel {channels[0]} or server {birthday.ServerDiscordId} is not available.");
                return;
            }

            await guild.DownloadUsersAsync();

            //So is a user who left the server
            IGuildUser? user = await guild.GetUserAsync(birthday.UserDiscordId, CacheMode.CacheOnly);
            if (user == null)
            {
                logger.Warning("BirthdayFeature.cs SendBirthdayMessageAsync", $"User {birthday.UserDiscordId} is not on server {birthday.ServerDiscordId} anymore, their birthday message is skipped.");
                return;
            }

            string message = BirthdayMessageProcessor.CreateMessage(birthday, user, currentYear);

            _ = await channel.SendMessageAsync(message);
        }
    }
}

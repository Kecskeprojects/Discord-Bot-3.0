using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord.WebSocket;
using Discord_Bot.Processors.MessageProcessor;
using Discord_Bot.Tools.NativeTools;

namespace Discord_Bot.Features;

public class BirthdayFeature(
    IBirthdayService birthdayService,
    DiscordSocketClient client,
    TimeProvider timeProvider,
    IServerService serverService,
    IBotLogger logger) : BaseFeature(serverService, logger)
{
    private readonly IBirthdayService birthdayService = birthdayService;
    private readonly DiscordSocketClient client = client;
    private readonly TimeProvider timeProvider = timeProvider;

    protected override async Task<bool> ExecuteCoreLogicAsync()
    {
        try
        {
            //Get the list of birthdays that are on this date
            DateTime today = timeProvider.GetUtcNow().UtcDateTime;
            List<BirthdayResource> result = await birthdayService.GetBirthdaysByDateAsync(today);
            if (!CollectionTools.IsNullOrEmpty(result))
            {
                logger.Log($"Sending birthday message to {result.Count} people.");
                foreach (BirthdayResource birthday in result)
                {
                    await SendBirthdayMessageAsync(birthday, today.Year);
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error("BirthdayFeature.cs ExecuteCoreLogicAsync", ex);
            return false;
        }
        return true;
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
            SocketGuild? guild = client.GetGuild(birthday.ServerDiscordId);

            //A deleted channel or a server the bot left is skipped, so the birthdays of the other servers are still sent
            if (client.GetChannel(channels[0]) is not ISocketMessageChannel channel || guild == null)
            {
                logger.Warning("BirthdayFeature.cs SendBirthdayMessageAsync", $"Birthday channel {channels[0]} or server {birthday.ServerDiscordId} is not available.");
                return;
            }

            await guild.DownloadUsersAsync();

            string message = BirthdayMessageProcessor.CreateMessage(birthday, guild, currentYear);

            _ = await channel.SendMessageAsync(message);
        }
    }
}

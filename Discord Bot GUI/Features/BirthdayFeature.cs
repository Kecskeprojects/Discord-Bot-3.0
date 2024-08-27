using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Processors.MessageProcessor;
using Discord_Bot.Resources;
using Discord_Bot.Tools.NativeTools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Features;

public class BirthdayFeature(
    IBirthdayService birthdayService,
    DiscordSocketClient client,
    IServerService serverService,
    BotLogger logger) : BaseFeature(serverService, logger)
{
    private readonly IBirthdayService birthdayService = birthdayService;
    private readonly DiscordSocketClient client = client;

    protected override async Task<bool> ExecuteCoreLogicAsync()
    {
        try
        {
            //Get the list of birthdays that are on this date
            List<BirthdayResource> result = await birthdayService.GetBirthdaysByDateAsync();
            if (!CollectionTools.IsNullOrEmpty(result))
            {
                logger.Log($"Sending birthday message to {result.Count} people.");
                foreach (BirthdayResource birthday in result)
                {
                    await SendBirthdayMessageAsync(birthday);
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

    private async Task SendBirthdayMessageAsync(BirthdayResource birthday)
    {
        ServerResource server = await serverService.GetByDiscordIdAsync(birthday.ServerDiscordId);

        if (server.SettingsChannels.TryGetValue(ChannelTypeEnum.BirthdayText, out List<ulong> channels))
        {
            ISocketMessageChannel channel = client.GetChannel(channels[0]) as ISocketMessageChannel;
            SocketGuild guild = client.GetGuild(birthday.ServerDiscordId);
            await guild.DownloadUsersAsync();

            string message = BirthdayMessageProcessor.CreateMessage(birthday, guild);

            await channel.SendMessageAsync(message);
        }
    }
}

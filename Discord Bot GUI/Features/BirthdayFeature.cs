using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using Discord_Bot.Tools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Features
{
    public class BirthdayFeature(IBirthdayService birthdayService, IServerService serverService, DiscordSocketClient client, Logging logger) : BaseFeature(logger)
    {
        private readonly IBirthdayService birthdayService = birthdayService;
        private readonly IServerService serverService = serverService;
        private readonly DiscordSocketClient client = client;

        protected override async Task ExecuteCoreLogicAsync()
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
                        ServerResource server = await serverService.GetByDiscordIdAsync(birthday.ServerDiscordId);

                        if (server.SettingsChannels.TryGetValue(ChannelTypeEnum.BirthdayText, out List<ulong> channels))
                        {
                            ISocketMessageChannel channel = client.GetChannel(channels[0]) as ISocketMessageChannel;
                            SocketGuild guild = client.GetGuild(birthday.ServerDiscordId);
                            await guild.DownloadUsersAsync();

                            string message = CreateBirthdayMessage(birthday, guild);

                            await channel.SendMessageAsync(message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("BirthdayFeature.cs ExecuteCoreLogicAsync", ex);
            }
        }
        private static string CreateBirthdayMessage(BirthdayResource birthday, SocketGuild guild)
        {
            SocketGuildUser user = guild.GetUser(birthday.UserDiscordId);

            Random r = new();
            string baseMessage = StaticLists.BirthdayMessage[r.Next(0, StaticLists.BirthdayMessage.Length)];

            string message = string.Format(baseMessage, user.Mention, (DateTime.UtcNow.Year - birthday.Date.Year).ToString());
            return message;
        }
    }
}

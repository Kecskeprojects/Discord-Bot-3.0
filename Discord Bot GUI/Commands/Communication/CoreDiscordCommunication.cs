﻿using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using Discord_Bot.CommandsService.Communication;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Commands.Communication;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Resources;
using Discord_Bot.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.Communication
{
    public class CoreDiscordCommunication(
        IServerService serverService,
        IReminderService reminderService,
        IBirthdayService birthdayService,
        IKeywordService keywordService,
        ICustomCommandService customCommandService,
        IRoleService roleService,
        IGreetingService greetingService,
        DiscordSocketClient client,
        IInstaLoader instaLoader,
        Logging logger) : ICoreToDiscordCommunication
    {
        private readonly IServerService serverService = serverService;
        private readonly IReminderService reminderService = reminderService;
        private readonly IBirthdayService birthdayService = birthdayService;
        private readonly IKeywordService keywordService = keywordService;
        private readonly ICustomCommandService customCommandService = customCommandService;
        private readonly IRoleService roleService = roleService;
        private readonly IGreetingService greetingService = greetingService;
        private readonly DiscordSocketClient client = client;
        private readonly IInstaLoader instaLoader = instaLoader;
        private readonly Logging logger = logger;

        #region Timer Based Communication
        public async Task SendBirthdayMessages()
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

                            string message = CoreToDiscordService.CreateBirthdayMessage(birthday, guild);

                            await channel.SendMessageAsync(message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("CoreDiscordCommunication.cs BirthdayCheck", ex.ToString());
            }
        }
        #endregion

        #region Core Interaction Communication
        public async Task SendReminders()
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
                            await user.SendMessageAsync(reminder.Message);
                        }
                    }

                    List<int> reminderIds = result.Select(r => r.ReminderId).ToList();
                    DbProcessResultEnum reminderResult = await reminderService.RemoveCurrentRemindersAsync(reminderIds);
                    if (reminderResult == DbProcessResultEnum.Failure)
                    {
                        logger.Error("CoreDiscordCommunication.cs ReminderCheck", "Failure during reminder check!");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("CoreDiscordCommunication.cs Log ReminderCheck", ex.ToString());
            }
        }

        public async Task CustomCommands(ICommandContext context)
        {
            try
            {
                CustomCommandResource command = await customCommandService.GetCustomCommandAsync(context.Guild.Id, context.Message.Content[1..].ToLower());
                if (command != null)
                {
                    await context.Channel.SendMessageAsync(command.Url);
                }
            }
            catch (Exception ex)
            {
                logger.Error("CoreDiscordCommunication.cs CustomCommands", ex.ToString());
            }
        }

        public async Task FeatureChecksAsync(SocketCommandContext context)
        {
            try
            {
                Random r = new();

                //Easter egg messages
                if (r.Next(0, 5000) == 0)
                {
                    await context.Channel.SendMessageAsync(StaticLists.EasterEggMessages[r.Next(0, StaticLists.EasterEggMessages.Length)]);
                    return;
                }
                else if (r.Next(1, 101) < 10)
                {
                    string mess = context.Message.Content.ToLower();
                    if (mess.StartsWith("i think"))
                    {
                        await context.Channel.SendMessageAsync("I agree wholeheartedly!");
                    }
                    else if ((mess.StartsWith("i am") && mess != "i am") || (mess.StartsWith("i'm") && mess != "i'm"))
                    {
                        await context.Channel.SendMessageAsync(string.Concat("Hey ", context.Message.Content.AsSpan(mess.StartsWith("i am") ? 5 : 4), ", I'm Kim Synthji!"));
                    }

                    return;
                }

                if (context.Message.Content.Length <= 100 && context.Channel.GetChannelType() != ChannelType.DM)
                {
                    KeywordResource keyword = await keywordService.GetKeywordAsync(context.Guild.Id, context.Message.Content.Replace("\'", "").Replace("\"", "").Replace("`", "").Replace(";", ""));
                    if (keyword != null)
                    {
                        await context.Channel.SendMessageAsync(keyword.Response);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("CoreDiscordCommunication.cs FeatureChecks", ex.ToString());
            }
        }

        public async Task SelfRole(SocketCommandContext context)
        {
            try
            {
                RoleResource role = await roleService.GetRoleAsync(context.Guild.Id, context.Message.Content[1..].ToLower());

                RestUserMessage reply = null;
                if (role != null)
                {
                    IRole discordRole = context.Guild.GetRole(role.DiscordId);

                    switch (context.Message.Content[0])
                    {
                        case '+':
                            {
                                await (context.User as SocketGuildUser).AddRoleAsync(discordRole);
                                reply = await context.Channel.SendMessageAsync($"You now have the `{discordRole.Name}` role!");
                                break;
                            }
                        case '-':
                            {
                                await (context.User as SocketGuildUser).RemoveRoleAsync(discordRole);
                                reply = await context.Channel.SendMessageAsync($"`{discordRole.Name}` role has been removed!");
                                break;
                            }
                    }
                }

                if (reply != null)
                {
                    await Task.Delay(1500);

                    await reply.DeleteAsync();
                }
            }
            catch (Exception ex)
            {
                logger.Error("CoreDiscordCommunication.cs SelfRole", ex.ToString());
            }
        }

        public async Task GreetAsync(SocketCommandContext context)
        {
            List<GreetingResource> list = await greetingService.GetAllGreetingAsync();
            if (!CollectionTools.IsNullOrEmpty(list))
            {
                await context.Channel.SendMessageAsync(list[new Random().Next(0, list.Count)].Url);
            }
        }

        public async Task SendInstagramPostEmbed(Uri uri, ulong messageId, ulong channelId, ulong? guildId)
        {
            IMessageChannel channel = client.GetChannel(channelId) as IMessageChannel;
            InstagramMessageResult result = new();

            List<FileAttachment> attachments = [];
            string postId = uri.Segments[2].EndsWith('/') ? uri.Segments[2][..^1] : uri.Segments[2];

            try
            {
                string errorDuringDownload = instaLoader.DownloadFromInstagram(postId);

                if (!string.IsNullOrEmpty(errorDuringDownload))
                {
                    logger.Error("ServiceDiscordCommunication.cs GetImagesFromPost", errorDuringDownload);
                    if (errorDuringDownload.Contains("Fetching Post metadata failed"))
                    {
                        await channel.SendMessageAsync("Posts cannot be accessed anonymously from this account.");
                    }
                }

                string[] files = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), $"Dependencies\\Instagram\\{postId}"));
                MessageReference refer = new(messageId, channelId, guildId, false);

                try
                {
                    result = await CoreToDiscordService.SendInstagramMessageAsync(attachments, files, uri.OriginalString, refer, channel, false);
                }
                catch (HttpException ex)
                {
                    logger.Warning("CoreDiscordCommunication.cs GetImagesFromPost", "Embed too large, only sending images!", LogOnly: true);
                    logger.Warning("CoreDiscordCommunication.cs GetImagesFromPost", ex.ToString(), LogOnly: true);

                    result = await CoreToDiscordService.SendInstagramMessageAsync(attachments, files, uri.OriginalString, refer, channel, true);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    logger.Warning("CoreDiscordCommunication.cs GetImagesFromPost", "Embed too large, only sending images!", LogOnly: true);
                    logger.Warning("CoreDiscordCommunication.cs GetImagesFromPost", ex.ToString(), LogOnly: true);

                    result = await CoreToDiscordService.SendInstagramMessageAsync(attachments, files, uri.OriginalString, refer, channel, true);
                }
            }
            catch (Exception ex)
            {
                logger.Error("CoreDiscordCommunication.cs GetImagesFromPost", ex.ToString());
                await channel.SendMessageAsync("Unexpected exception occured.");
            }

            if (result.HasFileDownloadHappened)
            {
                attachments.ForEach(x => x.Dispose());
                attachments.Clear();
                Directory.Delete(Path.Combine(Directory.GetCurrentDirectory(), $"Dependencies\\Instagram\\{postId}"), true);
            }

            if (result.ShouldMessageBeSuppressed)
            {
                IUserMessage discordMessage = await channel.GetMessageAsync(messageId) as IUserMessage;
                await discordMessage.ModifyAsync(x => x.Flags = MessageFlags.SuppressEmbeds);
            }
        }
        #endregion
    }
}

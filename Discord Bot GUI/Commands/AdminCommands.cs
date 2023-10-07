using Discord;
using Discord.Commands;
using Discord_Bot.Core.Config;
using Discord_Bot.Core.Logger;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Commands;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Resources;
using Discord_Bot.Services.Models.Twitch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    public class AdminCommands : CommandBase, IAdminCommands
    {
        private readonly IServerService serverService;
        private readonly IChannelService channelService;
        private readonly ITwitchChannelService twitchChannelService;
        private readonly ITwitchAPI twitchAPI;

        public AdminCommands(IServerService serverService, IChannelService channelService, ITwitchChannelService twitchChannelService, ITwitchAPI twitchAPI, Logging logger, Config config) : base(logger, config)
        {
            this.serverService = serverService;
            this.channelService = channelService;
            this.twitchChannelService = twitchChannelService;
            this.twitchAPI = twitchAPI;
        }

        [Command("help admin")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [RequireContext(ContextType.Guild)]
        [Summary("Embed complete list of commands in a text file")]
        public async Task Help()
        {
            try
            {
                if (!File.Exists("Assets\\Commands\\Admin_Commands.txt"))
                {
                    await ReplyAsync("Command file missing!");
                    return;
                }

                await Context.Channel.SendFileAsync(Directory.GetCurrentDirectory() + "\\Assets\\Commands\\Admin_Commands.txt");
            }
            catch (Exception ex)
            {
                logger.Error("AdminCommands.cs Help", ex.ToString());
            }
        }

        #region Server Setting
        [Command("channel add")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [RequireContext(ContextType.Guild)]
        [Summary("Server setting channel modification")]
        public async Task ChannelAdd(string type, [Remainder] string channelName)
        {
            try
            {
                IMessageChannel channel = Context.Guild.TextChannels.Where(x => x.Name.ToLower() == channelName.Trim().ToLower()).FirstOrDefault();

                if (channel == null)
                {
                    await ReplyAsync("Channel not found!");
                    return;
                }

                if (!ChannelTypeNameDictionary.NameEnum.ContainsKey(type))
                {
                    await ReplyAsync("Channel type does not currently exist\nCurrent types: " +
                        string.Join(", ", ChannelTypeNameDictionary.NameEnum.Keys.Select(x => $"`{x}`")));
                    return;
                }

                ChannelTypeEnum channelType = ChannelTypeNameDictionary.NameEnum[type];

                //Adds channel onto list of channels for server, unless conditions say there can only be one of a type of chat, removes server from cache
                DbProcessResultEnum result = await channelService.AddSettingChannelAsync(Context.Guild.Id, channelType, channel.Id);

                if (result == DbProcessResultEnum.Success)
                {
                    await ReplyAsync("Server settings updated!");
                }
                else if (result == DbProcessResultEnum.AlreadyExists)
                {
                    await ReplyAsync("Channel and type combination already in database!");
                }
                else
                {
                    await ReplyAsync("Server settings could not be updated!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("AdminCommands.cs ChannelAdd", ex.ToString());
            }
        }

        [Command("channel remove")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [RequireContext(ContextType.Guild)]
        [Summary("Server setting channel modification")]
        public async Task ChannelRemove(string type, [Remainder] string channelName = "all")
        {
            try
            {
                DbProcessResultEnum result;
                ChannelTypeEnum channelType = ChannelTypeNameDictionary.NameEnum[type];

                if (channelName == "all")
                {
                    //Removess every channel of a type that relates to the server, removes server from cache
                    result = await channelService.RemoveSettingChannelsAsync(Context.Guild.Id, channelType);
                }
                else
                {
                    IMessageChannel channel = Context.Guild.TextChannels.Where(x => x.Name.ToLower() == channelName.Trim().ToLower()).FirstOrDefault();

                    if (channel == null)
                    {
                        await ReplyAsync("Channel not found!");
                    }

                    if (!ChannelTypeNameDictionary.NameEnum.ContainsKey(type))
                    {
                        await ReplyAsync("Channel type does not currently exist\nCurrent types: " +
                            string.Join(", ", ChannelTypeNameDictionary.NameEnum.Keys.Select(x => $"`{x}`")));
                    }

                    //Removes the given channel of the given type from the settings for the server, removes server from cache
                    result = await channelService.RemovelSettingChanneAsync(Context.Guild.Id, channelType, channel.Id);
                }

                if (result == DbProcessResultEnum.Success)
                {
                    await ReplyAsync("Server settings updated!");
                }
                else if (result == DbProcessResultEnum.NotFound)
                {
                    await ReplyAsync("This channel is not currently set for this type!");
                }
                else
                {
                    await ReplyAsync("Server settings could not be updated!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("AdminCommands.cs ChannelRemove", ex.ToString());
            }
        }

        [Command("twitch add")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [RequireContext(ContextType.Guild)]
        [Summary("Server setting twitch related modification")]
        public async Task TwitchAdd(string type, [Remainder] string name)
        {
            try
            {
                DbProcessResultEnum result;

                if (type == "role")
                {
                    IRole role = Context.Guild.Roles.Where(x => x.Name.ToLower() == name).FirstOrDefault();
                    if (role == null)
                    {
                        await ReplyAsync("Role not found!");
                        return;
                    }
                    //Adds the notification role for every checked channel, as of now it will overwrite the previous one, there cannot be multiple, removes server from cache
                    result = await twitchChannelService.AddNotificationRoleAsync(Context.Guild.Id, role.Id);
                }
                else
                {
                    UserData response = twitchAPI.GetChannel(name);

                    if (response == null)
                    {
                        await ReplyAsync("Twitch user not found!");
                        return;
                    }

                    string userId = response.Id;
                    string url = $"https://www.twitch.tv/{response.Login}";

                    //Adds the twitch channel to the checked channel's list for the server, removes server from cache
                    result = await twitchChannelService.AddTwitchChannelAsync(Context.Guild.Id, userId, url);
                }

                if (result == DbProcessResultEnum.Success)
                {
                    await ReplyAsync("Server settings updated!");
                }
                else if (result == DbProcessResultEnum.AlreadyExists)
                {
                    if (type == "role")
                    {
                        await ReplyAsync("Notification role is the currently set one in the database for your server!");
                    }
                    else
                    {
                        await ReplyAsync("Twitch channel already in database for your server!");
                    }
                }
                else
                {
                    await ReplyAsync("Server settings could not be updated!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("AdminCommands.cs TwitchAdd", ex.ToString());
            }
        }

        [Command("twitch remove")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [RequireContext(ContextType.Guild)]
        [Summary("Server setting twitch related modification")]
        public async Task TwitchRemove(string type, [Remainder] string name = "all")
        {
            try
            {
                DbProcessResultEnum result;

                if (type == "role" && !string.IsNullOrEmpty(name))
                {
                    IRole role = Context.Guild.Roles.Where(x => x.Name.ToLower() == name).FirstOrDefault();
                    if (role == null)
                    {
                        await ReplyAsync("Role not found!");
                        return;
                    }
                    //Removes the currently set notification role, removes server from cache
                    result = await twitchChannelService.RemoveNotificationRoleAsync(Context.Guild.Id);
                }
                else
                {
                    if (!string.IsNullOrEmpty(name) && name == "all")
                    {
                        //Removes the twitch channel with the given name, removes server from cache
                        result = await twitchChannelService.RemoveTwitchChannelAsync(Context.Guild.Id, name);
                    }
                    else
                    {
                        //Removes every twitch channel tied to the server, removes server from cache
                        result = await twitchChannelService.RemoveTwitchChannelsAsync(Context.Guild.Id);
                    }
                }

                if (result == DbProcessResultEnum.Success)
                {
                    await ReplyAsync("Server settings updated!");
                }
                else if (result == DbProcessResultEnum.NotFound)
                {
                    await ReplyAsync("Twitch channel with that name not found in database!");
                }
                else
                {
                    await ReplyAsync("Server settings could not be updated!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("AdminCommands.cs TwitchRemove", ex.ToString());
            }
        }

        [Command("server settings")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [RequireContext(ContextType.Guild)]
        [Summary("Lists server settings")]
        public async Task ServerSettings()
        {
            try
            {
                ServerResource server = await serverService.GetByDiscordIdAsync(Context.Guild.Id);
                EmbedBuilder embed = new();

                embed.WithTitle("The server's settings are the following:");
                foreach (KeyValuePair<ChannelTypeEnum, string> item in ChannelTypeNameDictionary.EnumName)
                {
                    if (server.SettingsChannels.ContainsKey(item.Key))
                    {
                        IEnumerable<string> channels = server.SettingsChannels[item.Key]
                                                        .Select(x => Context.Guild.TextChannels
                                                                            .FirstOrDefault(n => n.Id == x))
                                                        .Where(x => x != null)
                                                        .Select(x => $"`{x.Name}`");

                        embed.AddField($"{item.Value}:", $"`{string.Join(", ", channels)}`");
                    }
                    else
                    {
                        embed.AddField($"{item.Value}:", "`none`");
                    }
                }

                if (server.TwitchChannels.Count > 0)
                {
                    embed.AddField("Notification role:", $"`{(server.TwitchChannels[0].RoleName != null ? server.TwitchChannels[0].RoleName : "none")}`");
                    embed.AddField("Notified Twitch Channel IDs:", $"`{string.Join(",", server.TwitchChannels.Select(x => x.TwitchId))}`");
                    embed.AddField("Notified Twitch channel URLs:", $"`{string.Join(",", server.TwitchChannels.Select(x => x.TwitchLink))}`");
                }
                else
                {
                    embed.AddField("Notification role:", $"`none`");
                    embed.AddField("Notified Twitch Channel IDs:", $"`none`");
                    embed.AddField("Notified Twitch channel URLs:", $"`none`");
                }

                embed.WithThumbnailUrl(config.Img);
                embed.WithTimestamp(DateTime.Now);
                embed.WithColor(Color.Teal);

                await ReplyAsync("", false, embed.Build());
            }
            catch (Exception ex)
            {
                logger.Error("AdminCommands.cs ServerSettings", ex.ToString());
            }
        }
        #endregion
    }
}

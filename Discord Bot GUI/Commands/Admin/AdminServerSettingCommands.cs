using Discord;
using Discord.Commands;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Processors.EmbedProcessors;
using Discord_Bot.Resources;
using Discord_Bot.Services.Models.Twitch;
using Discord_Bot.Tools;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.Admin;
public class AdminServerSettingCommands(
    IChannelService channelService,
    ITwitchChannelService twitchChannelService,
    ITwitchCLI twitchCLI,
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{
    private readonly IChannelService channelService = channelService;
    private readonly ITwitchChannelService twitchChannelService = twitchChannelService;
    private readonly ITwitchCLI twitchCLI = twitchCLI;

    [Command("server settings")]
    [Alias(["serversettings", "serversetting", "server setting"])]
    [RequireUserPermission(ChannelPermission.ManageChannels)]
    [RequireContext(ContextType.Guild)]
    [Summary("Lists server settings")]
    public async Task ServerSettings()
    {
        try
        {
            ServerResource server = await GetCurrentServerAsync();
            Embed[] embed = ServerSettingEmbedProcessor.CreateEmbed(server, Context.Guild.TextChannels, config.Img);

            await ReplyAsync(embeds: embed);
        }
        catch (Exception ex)
        {
            logger.Error("AdminServerSettingCommands.cs ServerSettings", ex);
        }
    }

    [Command("channel add")]
    [RequireUserPermission(ChannelPermission.ManageChannels)]
    [RequireContext(ContextType.Guild)]
    [Summary("Server setting channel modification")]
    public async Task ChannelAdd(string type = "", [Remainder] string channelName = "")
    {
        try
        {
            if (type == "" && channelName == "")
            {
                await ReplyAsync($"Current types: {string.Join(", ", ChannelTypeEnumTools.GetCommandArray())}");
                return;
            }

            if (channelName == "")
            {
                return;
            }

            IMessageChannel channel = Context.Guild.TextChannels.Where(x => x.Name.Equals(channelName.Trim(), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (channel == null)
            {
                await ReplyAsync("Channel not found!");
                return;
            }

            if (!ChannelTypeEnumTools.TryGetEnumFromCommandText(type, out ChannelTypeEnum? channelType))
            {
                await ReplyAsync($"Channel type does not currently exist\nCurrent types: {string.Join(", ", ChannelTypeEnumTools.GetCommandArray())}");
                return;
            }

            //Adds channel onto list of channels for server, unless conditions say there can only be one of a type of chat, removes server from cache
            DbProcessResultEnum result = await channelService.AddSettingChannelAsync(Context.Guild.Id, channelType.Value, channel.Id);
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => channelType.Value.IsRestrictedChannelType()
                                                ? "Server settings updated! Previous channel (if there was one) was overwritten as only one of it's type is allowed."
                                                : "Server settings updated.",
                DbProcessResultEnum.AlreadyExists => "Channel and type combination already in database.",
                DbProcessResultEnum.NotFound => "ChannelType or Server not found.",
                _ => "Server settings could not be updated!"
            };
            await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("AdminServerSettingCommands.cs ChannelAdd", ex);
        }
    }

    [Command("channel remove")]
    [RequireUserPermission(ChannelPermission.ManageChannels)]
    [RequireContext(ContextType.Guild)]
    [Summary("Server setting channel modification")]
    public async Task ChannelRemove(string type = "", [Remainder] string channelName = "all")
    {
        try
        {
            if (type == "" && channelName == "all")
            {
                await ReplyAsync($"Current types: {string.Join(", ", ChannelTypeEnumTools.GetCommandArray())}");
                return;
            }

            if (!ChannelTypeEnumTools.TryGetEnumFromCommandText(type, out ChannelTypeEnum? channelType))
            {
                await ReplyAsync($"Channel type does not currently exist\nCurrent types: {string.Join(", ", ChannelTypeEnumTools.GetCommandArray())}");
                return;
            }

            DbProcessResultEnum result;
            if (channelName == "all")
            {
                //Removess every channel of a type that relates to the server, removes server from cache
                result = await channelService.RemoveSettingChannelsAsync(Context.Guild.Id, channelType.Value);
            }
            else
            {
                IMessageChannel channel = Context.Guild.TextChannels.Where(x => x.Name.Equals(channelName.Trim(), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                if (channel == null)
                {
                    await ReplyAsync("Channel not found!");
                }

                //Removes the given channel of the given type from the settings for the server, removes server from cache
                result = await channelService.RemovelSettingChannelAsync(Context.Guild.Id, channelType.Value, channel.Id);
            }

            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => "Server settings updated.",
                DbProcessResultEnum.NotFound => "This channel is not currently set for this type.",
                _ => "Server settings could not be updated!"
            };
            await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("AdminServerSettingCommands.cs ChannelRemove", ex);
        }
    }

    [Command("twitch role add")]
    [RequireUserPermission(ChannelPermission.ManageChannels)]
    [RequireContext(ContextType.Guild)]
    [Summary("Server setting twitch role change")]
    public async Task TwitchRoleAdd([Remainder] string name)
    {
        try
        {
            IRole role = Context.Guild.Roles.Where(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (role == null)
            {
                await ReplyAsync("Role not found!");
                return;
            }
            //Adds the notification role for every checked channel, as of now it will overwrite the previous one, there cannot be multiple, removes server from cache
            DbProcessResultEnum result = await serverService.AddNotificationRoleAsync(Context.Guild.Id, role.Id, role.Name);
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => "Notification role updated.",
                DbProcessResultEnum.AlreadyExists => "Notification role is the currently set one in the database for your server.",
                _ => "Notification role could not be updated!"
            };
            await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("AdminServerSettingCommands.cs TwitchRoleAdd", ex);
        }
    }

    [Command("twitch role remove")]
    [RequireUserPermission(ChannelPermission.ManageChannels)]
    [RequireContext(ContextType.Guild)]
    [Summary("Server setting twitch role removal")]
    public async Task TwitchRoleRemove()
    {
        try
        {
            //Removes the currently set notification role, removes server from cache
            DbProcessResultEnum result = await serverService.RemoveNotificationRoleAsync(Context.Guild.Id);
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => "Notification role removed.",
                DbProcessResultEnum.NotFound => "Notification role currently not set in database.",
                _ => "Notification role could not be removed!"
            };
            await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("AdminServerSettingCommands.cs TwitchRoleRemove", ex);
        }
    }

    [Command("twitch add")]
    [RequireUserPermission(ChannelPermission.ManageChannels)]
    [RequireContext(ContextType.Guild)]
    [Summary("Server setting twitch channel addition")]
    public async Task TwitchAdd([Remainder] string name)
    {
        try
        {
            if (Uri.IsWellFormedUriString(name, UriKind.Absolute))
            {
                Uri uri = new(name);
                if (uri.Segments.Length < 2)
                {
                    await ReplyAsync("Url is not channel url!");
                    return;
                }
                name = uri.Segments[1].Replace("/", "");
            }
            UserData response = twitchCLI.GetChannel(name);

            if (response == null)
            {
                await ReplyAsync("Twitch user not found!");
                return;
            }

            string twitchUserId = response.Id;

            //Adds the twitch channel to the checked channel's list for the server, removes server from cache
            DbProcessResultEnum result = await twitchChannelService.AddTwitchChannelAsync(Context.Guild.Id, twitchUserId, response.Login);
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => "Twitch channel added.",
                DbProcessResultEnum.AlreadyExists => "Twitch channel already in database for your server.",
                _ => "Twitch channel could not be added!"
            };
            await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("AdminServerSettingCommands.cs TwitchAdd", ex);
        }
    }

    [Command("twitch remove")]
    [RequireUserPermission(ChannelPermission.ManageChannels)]
    [RequireContext(ContextType.Guild)]
    [Summary("Server setting twitch channel removal")]
    public async Task TwitchRemove([Remainder] string name = "all")
    {
        try
        {
            DbProcessResultEnum result;
            if (!string.IsNullOrEmpty(name) && name != "all")
            {
                if (Uri.IsWellFormedUriString(name, UriKind.Absolute))
                {
                    Uri uri = new(name);
                    if (uri.Segments.Length < 2)
                    {
                        await ReplyAsync("Url is not channel url!");
                        return;
                    }
                    name = uri.Segments[1].Replace("/", "");
                }
                //Removes the twitch channel with the given name, removes server from cache
                result = await twitchChannelService.RemoveTwitchChannelAsync(Context.Guild.Id, name);
            }
            else
            {
                //Removes every twitch channel tied to the server, removes server from cache
                result = await twitchChannelService.RemoveTwitchChannelAsync(Context.Guild.Id);
            }

            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => "Twitch channel(s) removed.",
                DbProcessResultEnum.NotFound => "Twitch channel with that name not found in database.",
                _ => "Twitch channel(s) could not be removed!"
            };
            await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("AdminServerSettingCommands.cs TwitchRemove", ex);
        }
    }
}

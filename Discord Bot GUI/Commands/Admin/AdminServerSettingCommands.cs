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
using System.Threading.Tasks;

namespace Discord_Bot.Commands.Admin;

[Name("Server Settings")]
[Remarks("Admin")]
[Summary("Managing server configuration")]
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
    [Alias(["serversettings", "server setting", "serversetting"])]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    [Summary("Details of the various settings on the server")]
    public async Task ServerSettings()
    {
        try
        {
            ServerResource server = await GetCurrentServerAsync();
            Embed[] embed = ServerSettingEmbedProcessor.CreateEmbed(server, Context.Guild.TextChannels, config.Img);

            _ = await ReplyAsync(embeds: embed);
        }
        catch (Exception ex)
        {
            logger.Error("AdminServerSettingCommands.cs ServerSettings", ex);
        }
    }

    [Command("channel add")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    [Summary("Set/add a type of channel for setting up/limiting other features")]
    public async Task ChannelAdd([Name("channel type")] string channeltype = "", [Remainder][Name("channel name")] IChannel channel = null)
    {
        try
        {
            if (channeltype == "")
            {
                _ = await ReplyAsync($"Current types: {string.Join(", ", ChannelTypeEnumTools.GetCommandArray())}");
                return;
            }

            if (!ChannelTypeEnumTools.TryGetEnumFromCommandText(channeltype, out ChannelTypeEnum? channelTypeEnum))
            {
                _ = await ReplyAsync($"Channel type does not currently exist.\nCurrent types: {string.Join(", ", ChannelTypeEnumTools.GetCommandArray())}");
                return;
            }

            if (channel == null)
            {
                return;
            }

            //Adds channel onto list of channels for server, unless conditions say there can only be one of a type of chat, removes server from cache
            DbProcessResultEnum result = await channelService.AddSettingChannelAsync(Context.Guild.Id, channelTypeEnum.Value, channel.Id);
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => channelTypeEnum.Value.IsRestrictedChannelType()
                                                ? "Server settings updated! Previous channel (if there was one) was overwritten as only one of it's type is allowed."
                                                : "Server settings updated.",
                DbProcessResultEnum.AlreadyExists => "Channel and type combination already in database.",
                DbProcessResultEnum.NotFound => "ChannelType or Server not found.",
                _ => "Server settings could not be updated!"
            };
            _ = await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("AdminServerSettingCommands.cs ChannelAdd", ex);
        }
    }

    [Command("channel remove")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    [Summary("Remove one or all set channels of a type\n*If no channel is given, all of given type will be removed")]
    public async Task ChannelRemove([Name("channel type")] string channeltype = "", [Remainder][Name("channel name*")] IChannel channel = null)
    {
        try
        {
            if (channeltype == "")
            {
                _ = await ReplyAsync($"Current types: {string.Join(", ", ChannelTypeEnumTools.GetCommandArray())}");
                return;
            }

            if (!ChannelTypeEnumTools.TryGetEnumFromCommandText(channeltype, out ChannelTypeEnum? channelTypeEnum))
            {
                _ = await ReplyAsync($"Channel type does not currently exist\nCurrent types: {string.Join(", ", ChannelTypeEnumTools.GetCommandArray())}");
                return;
            }

            DbProcessResultEnum result;
            if (channel == null)
            {
                //Removess every channel of a type that relates to the server, removes server from cache
                result = await channelService.RemoveSettingChannelsAsync(Context.Guild.Id, channelTypeEnum.Value);
            }
            else
            {
                //Removes the given channel of the given type from the settings for the server, removes server from cache
                result = await channelService.RemovelSettingChannelAsync(Context.Guild.Id, channelTypeEnum.Value, channel.Id);
            }

            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => "Server settings updated.",
                DbProcessResultEnum.NotFound => "This channel is not currently set for this type.",
                _ => "Server settings could not be updated!"
            };
            _ = await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("AdminServerSettingCommands.cs ChannelRemove", ex);
        }
    }

    [Command("twitch role add")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    [Summary("Add the role that will be notified upon any channel going online")]
    public async Task TwitchRoleAdd([Remainder][Name("role name")] IRole role)
    {
        try
        {
            //Adds the notification role for every checked channel, as of now it will overwrite the previous one, there cannot be multiple, removes server from cache
            DbProcessResultEnum result = await serverService.AddNotificationRoleAsync(Context.Guild.Id, role.Id, role.Name);
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => "Notification role updated.",
                DbProcessResultEnum.AlreadyExists => "Notification role is the currently set one in the database for your server.",
                _ => "Notification role could not be updated!"
            };
            _ = await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("AdminServerSettingCommands.cs TwitchRoleAdd", ex);
        }
    }

    [Command("twitch role remove")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    [Summary("Remove the role to notify users, notifications will be sent regardless")]
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
            _ = await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("AdminServerSettingCommands.cs TwitchRoleRemove", ex);
        }
    }

    [Command("twitch add")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    [Summary("Add a new twitch channel to be notified of on the server")]
    public async Task TwitchAdd([Name("twitch username/channel link")][Remainder] string twitchchannel)
    {
        try
        {
            if (Uri.IsWellFormedUriString(twitchchannel, UriKind.Absolute))
            {
                Uri uri = new(twitchchannel);
                if (uri.Segments.Length < 2)
                {
                    _ = await ReplyAsync("Url is not channel url!");
                    return;
                }
                twitchchannel = uri.Segments[1].Replace("/", "");
            }
            UserData response = twitchCLI.GetChannel(twitchchannel);

            if (response == null)
            {
                _ = await ReplyAsync("Twitch user not found!");
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
            _ = await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("AdminServerSettingCommands.cs TwitchAdd", ex);
        }
    }

    [Command("twitch remove")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    [Summary("Remove one or all channels currently checked for the server")]
    public async Task TwitchRemove([Name("twitch username/channel link")][Remainder] string twitchchannellink = "all")
    {
        try
        {
            DbProcessResultEnum result;
            if (!string.IsNullOrEmpty(twitchchannellink) && twitchchannellink != "all")
            {
                if (Uri.IsWellFormedUriString(twitchchannellink, UriKind.Absolute))
                {
                    Uri uri = new(twitchchannellink);
                    if (uri.Segments.Length < 2)
                    {
                        _ = await ReplyAsync("Url is not channel url!");
                        return;
                    }
                    twitchchannellink = uri.Segments[1].Replace("/", "");
                }
                //Removes the twitch channel with the given name, removes server from cache
                result = await twitchChannelService.RemoveTwitchChannelAsync(Context.Guild.Id, twitchchannellink);
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
            _ = await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("AdminServerSettingCommands.cs TwitchRemove", ex);
        }
    }
}

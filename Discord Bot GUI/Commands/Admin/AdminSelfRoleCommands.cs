using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Processors.MessageProcessor;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.Admin;

public class AdminSelfRoleCommands(
    IRoleService roleService,
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{
    private readonly IRoleService roleService = roleService;

    [Command("self role add")]
    [RequireUserPermission(ChannelPermission.ManageRoles)]
    [RequireContext(ContextType.Guild)]
    [Summary("Adding self role to database")]
    public async Task SelfRoleAdd([Remainder] string name)
    {
        try
        {
            //Check if role with that name exists
            IRole role = Context.Guild.Roles.Where(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (role != null)
            {
                DbProcessResultEnum result = await roleService.AddSelfRoleAsync(Context.Guild.Id, role.Name.ToLower(), role.Id);
                string resultMessage = result switch
                {
                    DbProcessResultEnum.Success => $"New role successfully added: {role.Name}.",
                    DbProcessResultEnum.AlreadyExists => "Role already in database.",
                    _ => "Role could not be added!"
                };
                await ReplyAsync(resultMessage);
            }
            else
            {
                await ReplyAsync("Role not found!");
            }
        }
        catch (Exception ex)
        {
            logger.Error("AdminSelfRoleCommands.cs SelfRoleAdd", ex);
        }
    }

    [Command("self role remove")]
    [RequireUserPermission(ChannelPermission.ManageRoles)]
    [RequireContext(ContextType.Guild)]
    [Summary("Removing self role from database")]
    public async Task SelfRoleRemove([Remainder] string name)
    {
        try
        {
            //Check if role with that name exists
            IRole role = Context.Guild.Roles.Where(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (role != null)
            {
                DbProcessResultEnum result = await roleService.RemoveSelfRoleAsync(Context.Guild.Id, name);
                string resultMessage = result switch
                {
                    DbProcessResultEnum.Success => $"The {role.Name} role has been removed.",
                    DbProcessResultEnum.NotFound => "Role could not be found.",
                    _ => "Role could not be removed!"
                };
                await ReplyAsync(resultMessage);
            }
            else
            {
                await ReplyAsync("Role does not exist!");
            }
        }
        catch (Exception ex)
        {
            logger.Error("AdminSelfRoleCommands.cs SelfRoleRemove", ex);
        }
    }

    [Command("update role message")]
    [RequireUserPermission(ChannelPermission.ManageRoles)]
    [RequireContext(ContextType.Guild)]
    [Summary("Sends a message of the roles that currently can be self assigned")]
    public async Task SendSelfRoleMessage()
    {
        try
        {
            ServerResource server = await GetCurrentServerAsync();

            if (!server.SettingsChannels.TryGetValue(ChannelTypeEnum.RoleText, out List<ulong> roleChannels))
            {
                return;
            }

            ISocketMessageChannel channel = Context.Client.GetChannel(roleChannels[0]) as ISocketMessageChannel;
            if (server.RoleMessageDiscordId.HasValue)
            {
                IMessage previousMessage = await channel.GetMessageAsync(server.RoleMessageDiscordId.Value);
                if (previousMessage != null)
                {
                    await channel.DeleteMessageAsync(previousMessage);
                }
            }

            List<RoleResource> roles = await roleService.GetServerRolesAsync(Context.Guild.Id);

            string message = RoleMessageProcessor.CreateMessage(roles);
            RestUserMessage newMessage = await channel.SendMessageAsync(message);

            DbProcessResultEnum result = await serverService.ChangeRoleMessageIdAsync(Context.Guild.Id, newMessage.Id);
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => "The role message has been updated.",
                _ => "Role message could not be updated!"
            };
            await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("AdminSelfRoleCommands.cs SendSelfRoleMessage", ex);
        }
    }
}

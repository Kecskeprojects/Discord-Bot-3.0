using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Features;

public class SelfRoleFeature(IRoleService roleService, IServerService serverService, BotLogger logger) : BaseFeature(serverService, logger)
{
    private readonly IRoleService roleService = roleService;

    protected override async Task<bool> ExecuteCoreLogicAsync()
    {
        try
        {
            RoleResource role = await roleService.GetRoleAsync(Context.Guild.Id, Context.Message.Content[1..].ToLower());

            RestUserMessage reply = null;
            if (role != null)
            {
                IRole discordRole = Context.Guild.GetRole(role.DiscordId);

                switch (Context.Message.Content[0])
                {
                    case '+':
                    {
                        await (Context.User as SocketGuildUser).AddRoleAsync(discordRole);
                        reply = await Context.Channel.SendMessageAsync($"You now have the `{discordRole.Name}` role!");
                        break;
                    }
                    case '-':
                    {
                        await (Context.User as SocketGuildUser).RemoveRoleAsync(discordRole);
                        reply = await Context.Channel.SendMessageAsync($"`{discordRole.Name}` role has been removed!");
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
            logger.Error("SelfRoleFeature.cs ExecuteCoreLogicAsync", ex);
            return false;
        }
        return true;
    }
}

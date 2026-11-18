using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord.Rest;
using Discord.WebSocket;

namespace Discord_Bot.Features;

public class SelfRoleFeature(
    IRoleService roleService,
    TimeProvider timeProvider,
    IServerService serverService,
    IBotLogger logger) : BaseFeature(serverService, logger)
{
    private readonly IRoleService roleService = roleService;
    private readonly TimeProvider timeProvider = timeProvider;

    protected override async Task<bool> ExecuteCoreLogicAsync()
    {
        try
        {
            RoleResource? role = await roleService.GetRoleAsync(Context.Guild.Id, Context.Message.Content[1..].ToLower());

            RestUserMessage? reply = null;
            if (role != null)
            {
                SocketRole discordRole = Context.Guild.GetRole(role.DiscordId);

                switch (Context.Message.Content[0])
                {
                    case '+':
                    {
                        await ((SocketGuildUser) Context.User).AddRoleAsync(discordRole);
                        reply = await Context.Channel.SendMessageAsync($"You now have the `{discordRole.Name}` role!");
                        break;
                    }
                    case '-':
                    {
                        await ((SocketGuildUser) Context.User).RemoveRoleAsync(discordRole);
                        reply = await Context.Channel.SendMessageAsync($"`{discordRole.Name}` role has been removed!");
                        break;
                    }
                }
            }

            if (reply != null)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1500), timeProvider);

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

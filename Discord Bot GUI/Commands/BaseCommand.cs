using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    public class BaseCommand(Logging logger, Config config, IServerService serverService) : ModuleBase<SocketCommandContext>
    {
        protected readonly Logging logger = logger;
        protected readonly Config config = config;
        protected readonly IServerService serverService = serverService;

        protected async Task<ServerResource> GetCurrentServerAsync()
        {
            return await serverService.GetByDiscordIdAsync(Context.Guild.Id);
        }

        protected string GetCurrentUserAvatar(ImageFormat format = ImageFormat.Png, ushort size = 512)
        {
            return Context.User.GetDisplayAvatarUrl(format, size);
        }

        protected string GetCurrentUserNickname()
        {
            return Context.Channel.GetChannelType() != ChannelType.DM
                ? (Context.User as SocketGuildUser).Nickname ?? Context.User.Username
                : Context.User.Username;
        }
    }
}

using Discord.Commands;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Commands
{
    public interface IAdminCommands
    {
        Task Help();
        Task ChannelAdd(string type, [Remainder] string channelName);
        Task ChannelRemove(string type, [Remainder] string channelName = "all");
        Task TwitchRoleAdd([Remainder] string name);
        Task TwitchAdd([Remainder] string name);
        Task TwitchRoleRemove();
        Task TwitchRemove([Remainder] string name = "all");
        Task ServerSettings();
    }
}

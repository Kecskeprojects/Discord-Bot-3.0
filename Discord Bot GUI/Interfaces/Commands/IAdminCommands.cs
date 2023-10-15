using Discord.Commands;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Commands
{
    public interface IAdminCommands
    {
        Task Help();
        Task ChannelAdd(string type, [Remainder] string channelName);
        Task ChannelRemove(string type, [Remainder] string channelName = "all");
        Task TwitchAdd(string type, [Remainder] string name);
        Task TwitchRemove(string type, [Remainder] string name = "all");
        Task ServerSettings();
    }
}
